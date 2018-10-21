﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore
{
    public class SqlCeComplianceTest : RelationalComplianceTestBase
    {
        protected override ICollection<Type> IgnoredTestBases { get; } = new HashSet<Type>
        {
            //Unsupported features in SQL Compact:
            typeof(AsyncFromSqlSprocQueryTestBase<>),
            typeof(FromSqlSprocQueryTestBase<>),
            typeof(SqlExecutorTestBase<>),
            typeof(SpatialTestBase<>),
            typeof(SpatialQueryTestBase<>),
            typeof(UdfDbFunctionTestBase<>),

            //No roundtrip of GUIDs with SQL CE
            typeof(StoreGeneratedFixupTestBase<>),

            //TODO ErikEJ await fixes to base test, see #513
            typeof(WithConstructorsTestBase<>),
            typeof(QueryFilterFuncletizationTestBase<>),

            //TODO Implement
            typeof(CompositeKeyEndToEndTestBase<>),
            typeof(QueryTaggingTestBase<>)
        };

        protected override Assembly TargetAssembly { get; } = typeof(SqlCeComplianceTest).Assembly;
    }
}
