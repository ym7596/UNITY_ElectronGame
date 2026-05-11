using VContainer;
using VContainer.Unity;
using Internal.Scripts.Input;
using Internal.Scripts.Core.GridSystem;
using Internal.Scripts.Core.TimeSystem;
using Internal.Scripts.Presenter;
using Internal.Scripts.UI;
using UnityEngine;


public class MainGameLifeTimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // 1. Input Service (Component)
        builder.RegisterComponentInHierarchy<InputManager>().As<IInputService>();
        
        // 2. Grid Service (Component)
        builder.RegisterComponentInHierarchy<GridManager>().As<IGridService>();

        // 3. Wire Placer (Component)
        builder.RegisterComponentInHierarchy<WirePlacer>();
        builder.RegisterComponentInHierarchy<GridVisualizer>();

        // 4. Time Service
        builder.RegisterEntryPoint<TimeManager>().As<ITimeService>();

        // 5. UI System
        builder.RegisterComponentInHierarchy<MainGameCanvas>().As<IUICanvas>();
        builder.RegisterEntryPoint<GameUIPresenter>(Lifetime.Scoped).AsSelf();

        // 6. Game Presenter (Entry Point)
        builder.RegisterEntryPoint<MainGameScenePresenter>(Lifetime.Scoped);

    }
}
