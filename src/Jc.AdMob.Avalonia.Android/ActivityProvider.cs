using Android.App;
using Android.OS;

namespace Jc.AdMob.Avalonia.Android;

internal static class ActivityProvider
{
    private static readonly object SyncLock = new();
    private static WeakReference<Activity>? _currentActivity;
    private static ActivityLifecycleCallbacks? _callbacks;

    internal static event EventHandler<Activity>? ActivityChanged;

    internal static Activity? CurrentActivity
    {
        get
        {
            if (_currentActivity?.TryGetTarget(out var activity) == true)
            {
                return activity;
            }

            return null;
        }
    }

    internal static void Initialize(Application application)
    {
        lock (SyncLock)
        {
            if (_callbacks is not null)
            {
                return;
            }

            _callbacks = new ActivityLifecycleCallbacks();
            application.RegisterActivityLifecycleCallbacks(_callbacks);
        }
    }

    private static void SetCurrentActivity(Activity activity)
    {
        _currentActivity = new WeakReference<Activity>(activity);
        ActivityChanged?.Invoke(null, activity);
    }

    private sealed class ActivityLifecycleCallbacks : Java.Lang.Object, Application.IActivityLifecycleCallbacks
    {
        public void OnActivityCreated(Activity activity, Bundle? savedInstanceState)
            => SetCurrentActivity(activity);

        public void OnActivityStarted(Activity activity)
            => SetCurrentActivity(activity);

        public void OnActivityResumed(Activity activity)
            => SetCurrentActivity(activity);

        public void OnActivityPaused(Activity activity)
        {
        }

        public void OnActivityStopped(Activity activity)
        {
        }

        public void OnActivitySaveInstanceState(Activity activity, Bundle outState)
        {
        }

        public void OnActivityDestroyed(Activity activity)
        {
            var current = CurrentActivity;
            if (ReferenceEquals(current, activity))
            {
                _currentActivity = null;
            }
        }
    }
}
