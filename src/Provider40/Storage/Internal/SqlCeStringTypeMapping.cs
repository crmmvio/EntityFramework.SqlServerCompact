﻿using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlServerCe;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;

namespace EFCore.SqlCe.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SqlCeStringTypeMapping : StringTypeMapping
    {
        private const int UnicodeMax = 4000;

        private readonly int _maxSpecificSize;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public SqlCeStringTypeMapping(
            [NotNull] string storeType,
            int? size = null,
            bool fixedLength = false,
            StoreTypePostfix? storeTypePostfix = null)
            : this(
                new RelationalTypeMappingParameters(
                    new CoreTypeMappingParameters(typeof(string)),
                    storeType,
                    GetStoreTypePostfix(storeTypePostfix, size),
                    GetDbType(fixedLength),
                    true,
                    size,
                    fixedLength))
        {
        }

        private static StoreTypePostfix GetStoreTypePostfix(StoreTypePostfix? storeTypePostfix, int? size)
            => storeTypePostfix
                ?? (size != null && size <= UnicodeMax ? StoreTypePostfix.Size : StoreTypePostfix.None);

        private static DbType? GetDbType(bool fixedLength) => fixedLength ? System.Data.DbType.String : (DbType?)null;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected SqlCeStringTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters)
        {
            _maxSpecificSize = CalculateSize(parameters.Size);
        }

        private static int CalculateSize(int? size)
            => size.HasValue && size <= UnicodeMax
                    ? size.Value
                    : UnicodeMax;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new SqlCeStringTypeMapping(parameters);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override void ConfigureParameter(DbParameter parameter)
        {
            //Set SqlDbType explicitly, to avoid errors like: Byte array truncation to a length of 8000
            //See https://connect.microsoft.com/sqlserver/feedback/details/311412/sqlceparameter-limits-dbtype-binary-to-8000-characters-ssce3-5

            // For strings and byte arrays, set the max length to the size facet if specified, or
            // 8000 bytes if no size facet specified, if the data will fit so as to avoid query cache
            // fragmentation by setting lots of different Size values otherwise always set to 
            // -1 (unbounded) to avoid SQL client size inference.

            var value = parameter.Value;
            var length = (value as string)?.Length;

            parameter.Size = (value == null) || (value == DBNull.Value) || ((length != null) && (length <= _maxSpecificSize))
                ? _maxSpecificSize
                : 0;

            if ((length == null) || (length.Value <= UnicodeMax))
            {
                parameter.DbType = System.Data.DbType.String;
                return;
            }
            ((SqlCeParameter)parameter).SqlDbType = SqlDbType.NText;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override string GenerateNonNullSqlLiteral(object value)
            => $"N'{EscapeSqlLiteral((string)value)}'"; // Interpolation okay; strings
    }
}
