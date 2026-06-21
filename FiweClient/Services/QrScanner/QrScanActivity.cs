#if ANDROID
#pragma warning disable CS0618  // Camera API deprecated but still functional
#pragma warning disable CA1416  // Runtime permissions (API 23+) — app targets modern Android
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Hardware;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.App;
using ZXing;
using ZXing.Common;
using ActivityResult = Android.App.Result;

namespace FiweClient.Services.QrScanner;

[Activity(
    Theme = "@style/Theme.AppCompat.NoActionBar",
    ScreenOrientation = ScreenOrientation.Portrait,
    Label = "Сканирование QR")]
public sealed class QrScanActivity : AppCompatActivity, ISurfaceHolderCallback, Camera.IPreviewCallback
{
    private Camera? _camera;
    private SurfaceView? _surfaceView;
    private readonly ZXing.BarcodeReaderGeneric _reader = new();
    private volatile bool _isProcessing;
    private const int CameraRequestCode = 100;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        _surfaceView = new SurfaceView(this);
        SetContentView(_surfaceView);
        _surfaceView.Holder!.AddCallback(this);
    }

    protected override void OnResume()
    {
        base.OnResume();
        if (CheckSelfPermission(Android.Manifest.Permission.Camera) == Permission.Granted)
            TryOpenCamera();
        else
            RequestPermissions([Android.Manifest.Permission.Camera], CameraRequestCode);
    }

    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
    {
        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        if (requestCode != CameraRequestCode) return;

        if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
            TryOpenCamera();
        else
            Cancel();
    }

    // ── ISurfaceHolderCallback ────────────────────────────────────────

    public void SurfaceCreated(ISurfaceHolder holder)
    {
        if (CheckSelfPermission(Android.Manifest.Permission.Camera) == Permission.Granted)
            OpenCamera(holder);
    }

    public void SurfaceChanged(ISurfaceHolder holder, Android.Graphics.Format format, int width, int height) { }

    public void SurfaceDestroyed(ISurfaceHolder holder) => ReleaseCamera();

    // ── Camera ────────────────────────────────────────────────────────

    private void TryOpenCamera()
    {
        if (_camera != null || _surfaceView?.Holder == null) return;
        OpenCamera(_surfaceView.Holder);
    }

    private void OpenCamera(ISurfaceHolder holder)
    {
        if (_camera != null) return;
        try
        {
            _camera = Camera.Open();
            _camera!.SetDisplayOrientation(90);

            var parameters = _camera.GetParameters();
            // Выбираем разрешение preview ближайшее к 640×480 (баланс качества и скорости)
            var sizes = parameters?.SupportedPreviewSizes;
            if (sizes != null && sizes.Count > 0)
            {
                var best = sizes.OrderBy(s => Math.Abs(s.Width * s.Height - 640 * 480)).First();
                parameters!.SetPreviewSize(best.Width, best.Height);
                _camera.SetParameters(parameters);
            }

            _camera.SetPreviewDisplay(holder);
            _camera.SetPreviewCallback(this);
            _camera.StartPreview();
        }
        catch
        {
            Cancel();
        }
    }

    private void ReleaseCamera()
    {
        _camera?.SetPreviewCallback(null);
        _camera?.StopPreview();
        _camera?.Release();
        _camera = null;
    }

    // ── Camera.IPreviewCallback ───────────────────────────────────────

    public void OnPreviewFrame(byte[]? data, Camera? camera)
    {
        if (data == null || _isProcessing) return;
        _isProcessing = true;

        var parameters = camera?.GetParameters();
        if (parameters == null) { _isProcessing = false; return; }

        var width = parameters.PreviewSize!.Width;
        var height = parameters.PreviewSize.Height;

        // Копируем только Y-плоскость NV21 (grayscale), чтобы не удерживать буфер камеры
        var luminance = new byte[width * height];
        Buffer.BlockCopy(data, 0, luminance, 0, luminance.Length);

        Task.Run(() =>
        {
            try
            {
                var source = new RGBLuminanceSource(
                    luminance, width, height, RGBLuminanceSource.BitmapFormat.Gray8);
                var result = _reader.Decode(source);
                if (result != null)
                {
                    var intent = new Intent();
                    intent.PutExtra("qr_result", result.Text);
                    SetResult(ActivityResult.Ok, intent);
                    RunOnUiThread(Finish);
                    return;
                }
            }
            catch { }
            _isProcessing = false;
        });
    }

    protected override void OnDestroy()
    {
        ReleaseCamera();
        base.OnDestroy();
    }

    private void Cancel()
    {
        SetResult(ActivityResult.Canceled);
        Finish();
    }
}
#endif
