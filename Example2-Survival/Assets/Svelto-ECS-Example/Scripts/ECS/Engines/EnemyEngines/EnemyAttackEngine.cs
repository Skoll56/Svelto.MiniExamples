using System.Collections;
using Svelto.Tasks;
using UnityEngine;

namespace Svelto.ECS.Example.Survive.Characters.Enemies
{
    public class EnemyAttackEngine : SingleEntityEngine<EnemyTargetEntityViewStruct>, IQueryingEntitiesEngine
    {
        readonly ITaskRoutine<IEnumerator> _taskRoutine;


        readonly ITime _time;

        public EnemyAttackEngine(ITime time)
        {
            _time        = time;
            _taskRoutine = TaskRunner.Instance.AllocateNewTaskRoutine(StandardSchedulers.physicScheduler);
            _taskRoutine.SetEnumerator(CheckIfHittingEnemyTarget());
        }

        public IEntitiesDB entitiesDB { set; private get; }

        public void Ready() { }

        protected override void Add(ref EnemyTargetEntityViewStruct      entityView,
                                    ExclusiveGroup.ExclusiveGroupStruct? previousGroup)
        {
            _taskRoutine.Start();
        }

        protected override void Remove(ref EnemyTargetEntityViewStruct entityView, bool itsaSwap)
        {
            _taskRoutine.Stop();
        }

        IEnumerator CheckIfHittingEnemyTarget()
        {
            while (true)
            {
                // Pay attention to this bit. The engine is querying a
                // EnemyTargetEntityView and not a PlayerEntityView.
                // this is more than a sophistication, it actually the implementation
                // of the rule that every engine must use its own set of
                // EntityViews to promote encapsulation and modularity
                while (entitiesDB.HasAny<DamageableEntityStruct>(ECSGroups.EnemyTargets) == false ||
                       entitiesDB.HasAny<EnemyAttackEntityView>(ECSGroups.ActiveEnemies) == false)
                    yield return null;

                var targetEntities =
                    entitiesDB.QueryEntities<DamageableEntityStruct>(ECSGroups.EnemyTargets, out var targetsCount);

                var enemiesAttackData =
                    entitiesDB.QueryEntities<EnemyAttackStruct>(ECSGroups.ActiveEnemies, out var enemiesCount);
                var enemies =
                    entitiesDB.QueryEntities<EnemyAttackEntityView>(ECSGroups.ActiveEnemies, out enemiesCount);

                //this is more complex than needed code is just to show how you can use entity structs
                //this case is banal, entity structs should be use to handle hundreds or thousands
                //of entities in a cache friendly and multi threaded code. However entity structs would allow
                //the creation of entity without any allocation, so they can be handy for
                //cases where entity should be built fast! Theoretically is possible to create
                //a game using only entity structs, but entity structs make sense ONLY if they
                //hold value types, so they come with a lot of limitations
                for (var enemyIndex = 0; enemyIndex < enemiesCount; enemyIndex++)
                {
                    var enemyAttackEntityView = enemies[enemyIndex];

                    enemiesAttackData[enemyIndex].entityInRange =
                        enemyAttackEntityView.targetTriggerComponent.entityInRange;
                }

                for (var enemyTargetIndex = 0; enemyTargetIndex < targetsCount; enemyTargetIndex++)
                {
                    var targetEntityView = targetEntities[enemyTargetIndex];

                    for (var enemyIndex = 0; enemyIndex < enemiesCount; enemyIndex++)
                        if (enemiesAttackData[enemyIndex].entityInRange.collides)
                            if (enemiesAttackData[enemyIndex].entityInRange.otherEntityID == targetEntityView.ID)
                            {
                                enemiesAttackData[enemyIndex].timer += _time.deltaTime;

                                if (enemiesAttackData[enemyIndex].timer >=
                                    enemiesAttackData[enemyIndex].timeBetweenAttack)
                                {
                                    enemiesAttackData[enemyIndex].timer = 0.0f;

                                    targetEntities[enemyTargetIndex].damageInfo =
                                        new DamageInfo(enemiesAttackData[enemyIndex].attackDamage, Vector3.zero);
                                }
                            }
                }

                yield return null;
            }
        }
    }
}