﻿// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Linq;
using EdFi.Db.Deploy.Helpers;
using EdFi.Db.Deploy.Parameters;

namespace EdFi.Db.Deploy.Specifications
{
    public class ConnectionStringSpecification : OptionsSpecification
    {
        public override bool IsSatisfiedBy(IOptions obj)
        {
            Preconditions.ThrowIfNull(obj, nameof(obj));

            if (obj.ConnectionString == null)
            {
                ErrorMessages.Add("Connection string is null;");
            }

            if (obj.ConnectionString == string.Empty)
            {
                ErrorMessages.Add("Connection string is empty");
            }

            return !ErrorMessages.Any();
        }
    }
}
