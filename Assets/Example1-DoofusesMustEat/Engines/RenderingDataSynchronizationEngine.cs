using System;
using System.Collections;
using Svelto.ECS.EntityStructs;
using Svelto.Tasks.ExtraLean;
using Svelto.Tasks.ExtraLean.Unity;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Svelto.ECS.MiniExamples.Example1
{
    public class RenderingDataSynchronizationEngine: IDisposable, IQueryingEntitiesEngine
    {
        public IEntitiesDB entitiesDB { get; set; }

        public RenderingDataSynchronizationEngine(World world)
        {
            _runner      = new CoroutineMonoRunner("test");
            _group       = world.EntityManager.CreateComponentGroup(typeof(Translation), typeof(UnityECSDoofusesGroup));
        }

        public void Ready() { SynchronizeUnityECSEntitiesWithSveltoECSEntities().RunOn(_runner); }

        IEnumerator SynchronizeUnityECSEntitiesWithSveltoECSEntities()
        {
            while (true)
            {
                var calculateLength = _group.CalculateLength();
                
                var positionEntityStructs =
                    entitiesDB.QueryEntities<PositionEntityStruct>(GameGroups.DOOFUSES, out var count);

                if (calculateLength != count) {yield return null; continue;}

                var positions =
                    new NativeArray<Translation>(calculateLength, Allocator.TempJob,
                                                 NativeArrayOptions.UninitializedMemory);
                
                for (int index = 0; index < count; index++)
                {
                    positions[index] = new Translation
                    {
                        Value = new float3(positionEntityStructs[index].position.x,
                                           positionEntityStructs[index].position.y,
                                           positionEntityStructs[index].position.z)
                    };
                }
                //UnityECS: Why I cannot set the number of items I want? I could avoid creating the native array every
                //frame!
                //Also I cannot find a way to iterate over the chunks linearly (with the operator[]). It would
                //be better to access the components directly.
                _group.CopyFromComponentDataArray(positions, out var handle3);

                handle3.Complete();
                positions.Dispose();

                yield return null;
            }
        }

        public void Dispose() { _runner?.Dispose(); }

        readonly CoroutineMonoRunner _runner;
        readonly ComponentGroup      _group;
    }
}