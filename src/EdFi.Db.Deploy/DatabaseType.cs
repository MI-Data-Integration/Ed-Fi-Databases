// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace EdFi.Db.Deploy
{

    public class DatabaseType: IEquatable<DatabaseType>
    {
        public static DatabaseType Admin
        {
            get
            {
                return new DatabaseType("Admin");
            }
        }

        public static DatabaseType ODS
        {
            get
            {
                return new DatabaseType("ODS");
            }
        }

        public static DatabaseType Security
        {
            get
            {
                return new DatabaseType("Security");
            }
        }
        private readonly string _value;
        public DatabaseType(string value)
        {
            this._value = value;
        }
        public override string ToString()
        {
            return _value;
        }

        public static implicit operator string(DatabaseType d)
        {
            return d._value;
        }
        public static implicit operator DatabaseType(string d)
        {
            return new DatabaseType(d);
        }
        public bool Equals(DatabaseType other)
        {
            return _value == other.ToString();
        }
        public static bool Equals(DatabaseType d1, DatabaseType d2)
        {
            return d1.Equals(d2);
        }
        public static bool operator ==(DatabaseType d1, DatabaseType d2)
        {
            return d1.Equals(d2);
        }
        public static bool operator !=(DatabaseType d1, DatabaseType d2)
        {
            return !d1.Equals(d2);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DatabaseType);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }
    }

    public static class DatabaseTypeComparer
    {
        public static IEqualityComparer<DatabaseType> InvariantCulture => new DatabaseTypeStringComparer(StringComparer.InvariantCulture);
        public static IEqualityComparer<DatabaseType> InvariantCultureIgnoreCase => new DatabaseTypeStringComparer(StringComparer.InvariantCultureIgnoreCase);
        public static IEqualityComparer<DatabaseType> CurrentCulture => new DatabaseTypeStringComparer(StringComparer.CurrentCulture);
        public static IEqualityComparer<DatabaseType> CurrentCultureIgnoreCase => new DatabaseTypeStringComparer(StringComparer.CurrentCultureIgnoreCase);
        public static IEqualityComparer<DatabaseType> Ordinal => new DatabaseTypeStringComparer(StringComparer.Ordinal);
        public static IEqualityComparer<DatabaseType> OrdinalIgnoreCase => new DatabaseTypeStringComparer(StringComparer.OrdinalIgnoreCase);
    }

    public class DatabaseTypeStringComparer : IEqualityComparer<DatabaseType>
    {
        private IEqualityComparer<String> _comparer;
        public DatabaseTypeStringComparer (IEqualityComparer<String> comparer)
        {
            _comparer = comparer;
        }
        public bool Equals(DatabaseType x, DatabaseType y)
        {
            return x.ToString().Equals(y.ToString(), StringComparison.InvariantCulture);
        }

        public int GetHashCode([DisallowNull] DatabaseType obj) => obj.GetHashCode();
    }


}
