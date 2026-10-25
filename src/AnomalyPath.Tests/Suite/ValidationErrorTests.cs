#nullable enable
using System.Collections.Generic;
using System.IO;
using AnomalyPath.Core;
using AnomalyPath.Core.Validation;

namespace AnomalyPath.Tests
{
    public sealed partial class ValidationErrorTests : TestBase
    {
        private readonly string _dataRoot;
        public ValidationErrorTests(string dataRoot)
        {
            _dataRoot = dataRoot;
        }

        public override void Run()
        {
            ErrorTests(); ErrorBehaviorTests(); RouteErrorTests(); TraderErrorTests(); QuestErrorTests(); TalentErrorTests(); LootErrorTests(); SpawnErrorTests();
        }
    }
}
