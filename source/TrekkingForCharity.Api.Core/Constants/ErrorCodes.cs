// Copyright 2017 Trekking for Charity
// This file is part of TrekkingForCharity.Api.
// TrekkingForCharity.Api is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// TrekkingForCharity.Api is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with TrekkingForCharity.Api. If not, see http://www.gnu.org/licenses/.

namespace TrekkingForCharity.Api.Core.Constants
{
    public enum ErrorCodes
    {
        UntypedError = -1,
        NoError = 0,
        Validation = 1,
        TrekNotFound = 2,
        Creation = 3,
        TrekNameInUse = 4,
        WaypointNotFound = 5,
        NotAuthenticated = 6,
        CommandIsNotSet = 7,
        Deletion = 8,
        UpdatesNotFound = 9,
        QueryIsNotSet = 10
    }
}
