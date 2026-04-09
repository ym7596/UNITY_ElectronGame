using UnityEngine;
using VContainer;
using VContainer.Unity;
using Internal.Scripts.Input;
using Internal.Scripts.Core.ElecSystem;

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

        // 4. Game Presenter (Entry Point)
        builder.RegisterEntryPoint<MainGameScenePresenter>(Lifetime.Scoped);
    }
}
