using Svelto.ECS.EntityComponents;

namespace Svelto.ECS.MiniExamples.Example1C
{
    class DoofusEntityDescriptor
        : GenericEntityDescriptor<PositionEntityComponent, UnityEcsEntityComponent, VelocityEntityComponent,
            SpeedEntityComponent, EGIDComponent>
    {
    }
}