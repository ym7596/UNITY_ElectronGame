using UnityEngine;
using VContainer;
using VContainer.Unity;

public class MainGameLifeTimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<InputManager>().As<IInputService>();
        builder.RegisterEntryPoint<MainGameScenePresenter>(Lifetime.Scoped);
    }
}
