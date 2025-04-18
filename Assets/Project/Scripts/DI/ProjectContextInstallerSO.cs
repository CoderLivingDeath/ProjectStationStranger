using Assets.Project.Scripts.Infrastructure.EventBus;
using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "ProjectContextInstallerSO", menuName = "Installers/ProjectContextInstallerSO")]
public class ProjectContextInstallerSO : ScriptableObjectInstaller<ProjectContextInstallerSO>
{
    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<EventBus>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<InputSystem_Actions>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<InputService>().AsSingle().NonLazy();
    }
}