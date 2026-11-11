using AVFoundation;
using CoreFoundation;
using Foundation;
using UIKit;

namespace FiweClient.iOS;

/// <summary>
/// Полноэкранный сканер QR-кода камерой (AVFoundation) — аналог QrScanActivity на Android.
/// Используется на экране переноса ключей: сканирует QR, показанный старым устройством.
/// </summary>
public sealed class QrScanViewController : UIViewController
{
    private readonly TaskCompletionSource<string?> _result = new();
    private AVCaptureSession? _session;
    private AVCaptureVideoPreviewLayer? _preview;
    private MetadataDelegate? _metadataDelegate;
    private bool _finished;
    private bool _cameraFailed;

    /// <summary>
    /// Запрашивает доступ к камере, показывает сканер и возвращает текст QR-кода
    /// (null — пользователь отменил или камера недоступна).
    /// </summary>
    public static Task<string?> ScanAsync()
    {
        var tcs = new TaskCompletionSource<string?>();

        // UIKit можно трогать только из главного потока
        UIApplication.SharedApplication.BeginInvokeOnMainThread(async () =>
        {
            try
            {
                if (!await EnsureCameraAccessAsync())
                {
                    tcs.TrySetResult(null);
                    return;
                }

                var presenter = GetTopViewController();
                if (presenter is null)
                {
                    tcs.TrySetResult(null);
                    return;
                }

                var scanner = new QrScanViewController
                {
                    ModalPresentationStyle = UIModalPresentationStyle.FullScreen,
                };
                await presenter.PresentViewControllerAsync(scanner, true);
                tcs.TrySetResult(await scanner._result.Task);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== FiweClient iOS QR scan failed: {ex} ===");
                tcs.TrySetResult(null);
            }
        });

        return tcs.Task;
    }

    private static async Task<bool> EnsureCameraAccessAsync()
    {
        var status = AVCaptureDevice.GetAuthorizationStatus(AVAuthorizationMediaType.Video);
        return status switch
        {
            AVAuthorizationStatus.Authorized => true,
            AVAuthorizationStatus.NotDetermined =>
                await AVCaptureDevice.RequestAccessForMediaTypeAsync(AVAuthorizationMediaType.Video),
            _ => false, // доступ запрещён в настройках — сканер не открываем, остаётся ввод кода вручную
        };
    }

    private static UIViewController? GetTopViewController()
    {
        var window = UIApplication.SharedApplication.ConnectedScenes
            .OfType<UIWindowScene>()
            .SelectMany(scene => scene.Windows)
            .FirstOrDefault(w => w.IsKeyWindow);

        var controller = window?.RootViewController;
        while (controller?.PresentedViewController is { } presented)
            controller = presented;
        return controller;
    }

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();
        View!.BackgroundColor = UIColor.Black;

        _cameraFailed = !TrySetupCamera();
        AddOverlay();
    }

    // Закрывать контроллер можно только после окончания анимации показа — иначе UIKit проигнорирует dismiss
    public override void ViewDidAppear(bool animated)
    {
        base.ViewDidAppear(animated);
        if (_cameraFailed)
            Finish(null);
    }

    private bool TrySetupCamera()
    {
        var device = AVCaptureDevice.GetDefaultDevice(AVMediaTypes.Video);
        if (device is null)
            return false; // симулятор: камеры нет

        var input = AVCaptureDeviceInput.FromDevice(device, out _);
        if (input is null)
            return false;

        _session = new AVCaptureSession();
        if (!_session.CanAddInput(input))
            return false;
        _session.AddInput(input);

        var output = new AVCaptureMetadataOutput();
        if (!_session.CanAddOutput(output))
            return false;
        _session.AddOutput(output);

        // Типы кодов можно задать только после добавления output в сессию
        _metadataDelegate = new MetadataDelegate(this);
        output.SetDelegate(_metadataDelegate, DispatchQueue.MainQueue);
        output.MetadataObjectTypes = AVMetadataObjectType.QRCode;

        _preview = new AVCaptureVideoPreviewLayer(_session)
        {
            VideoGravity = AVLayerVideoGravity.ResizeAspectFill,
            Frame = View!.Bounds,
        };
        View.Layer.AddSublayer(_preview);
        return true;
    }

    private void AddOverlay()
    {
        var hint = new UILabel
        {
            Text = "Наведите камеру на QR-код\nна старом устройстве",
            TextColor = UIColor.White,
            TextAlignment = UITextAlignment.Center,
            Lines = 0,
            Font = UIFont.SystemFontOfSize(16, UIFontWeight.Semibold),
            TranslatesAutoresizingMaskIntoConstraints = false,
        };

        var cancel = UIButton.FromType(UIButtonType.System);
        cancel.SetTitle("Отмена", UIControlState.Normal);
        cancel.SetTitleColor(UIColor.White, UIControlState.Normal);
        cancel.TitleLabel!.Font = UIFont.SystemFontOfSize(17, UIFontWeight.Semibold);
        cancel.TranslatesAutoresizingMaskIntoConstraints = false;
        cancel.TouchUpInside += (_, _) => Finish(null);

        View!.AddSubview(hint);
        View.AddSubview(cancel);

        var safe = View.SafeAreaLayoutGuide;
        NSLayoutConstraint.ActivateConstraints(
        [
            hint.TopAnchor.ConstraintEqualTo(safe.TopAnchor, 24),
            hint.LeadingAnchor.ConstraintEqualTo(safe.LeadingAnchor, 24),
            hint.TrailingAnchor.ConstraintEqualTo(safe.TrailingAnchor, -24),
            cancel.BottomAnchor.ConstraintEqualTo(safe.BottomAnchor, -24),
            cancel.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor),
        ]);
    }

    public override void ViewDidLayoutSubviews()
    {
        base.ViewDidLayoutSubviews();
        if (_preview is not null)
            _preview.Frame = View!.Bounds;
    }

    public override void ViewWillAppear(bool animated)
    {
        base.ViewWillAppear(animated);
        // StartRunning блокирует поток — не держим главный
        var session = _session;
        if (session is not null)
            Task.Run(session.StartRunning);
    }

    public override void ViewWillDisappear(bool animated)
    {
        base.ViewWillDisappear(animated);
        _session?.StopRunning();
    }

    private void Finish(string? code)
    {
        if (_finished) return;
        _finished = true;

        _session?.StopRunning();

        BeginInvokeOnMainThread(() =>
            DismissViewController(true, () => _result.TrySetResult(code)));
    }

    private sealed class MetadataDelegate(QrScanViewController owner) : AVCaptureMetadataOutputObjectsDelegate
    {
        public override void DidOutputMetadataObjects(
            AVCaptureMetadataOutput captureOutput,
            AVMetadataObject[] metadataObjects,
            AVCaptureConnection connection)
        {
            var code = metadataObjects
                .OfType<AVMetadataMachineReadableCodeObject>()
                .Select(o => o.StringValue)
                .FirstOrDefault(s => !string.IsNullOrEmpty(s));

            if (code is not null)
                owner.Finish(code);
        }
    }
}
