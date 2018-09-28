// Copyright 2017 Trekking for Charity
// This file is part of TrekkingForCharity.Api.
// TrekkingForCharity.Api is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// TrekkingForCharity.Api is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with TrekkingForCharity.Api. If not, see http://www.gnu.org/licenses/.

namespace TrekkingForCharity.Api.Core.Constants
{
    public static class ErrorCodes
    {
        public const string UntypedError = "ERR-000000";
        public const string Validation = "ERR-000001";
        public const string TrekNotFound = "ERR-000002";
        public const string Creation = "ERR-000003";
        public const string TrekNameInUse = "ERR-000004";
        public const string WaypointNotFound = "ERR-000005";
        public const string NotAuthenticated = "ERR-000006";
        public const string CommandIsNotSet = "ERR-000007";
        public const string Deletion = "ERR-000008";
        public const string UpdatesNotFound = "ERR-000009";
        public const string QueryIsNotSet = "ERR-000010";
    }
}