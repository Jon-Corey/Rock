/* eslint-disable @typescript-eslint/naming-convention */

// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

/**
 * Information about a scheduler resource assignment for the group scheduler.
 * Represenation of: https://github.com/SparkDevNetwork/Rock/blob/8dfb45edcbf4f166d483f6e96ed39806f3ca6a1b/Rock/Model/Event/Attendance/AttendanceService.cs#L3107
 */
export interface ISchedulerResourceAssignment {
    /** The group identifier. */
    GroupId: number,

    /** The name of the group. */
    GroupName?: string | null,

    /** The schedule identifier. */
    ScheduleId: number,

    /** The naem of the schedule. */
    ScheduleName?: string | null,

    /** The location identifier. */
    LocationId?: number | null,

    /** The name of the location. */
    LocationName?: string | null,

    /** The occurrence date. */
    OccurrenceDate?: string | null
}

/**
 * Information about a potential scheduler resource (Person) for the group scheduler.
 * Represenation of: https://github.com/SparkDevNetwork/Rock/blob/8dfb45edcbf4f166d483f6e96ed39806f3ca6a1b/Rock/Model/Event/Attendance/AttendanceService.cs#L3169
 */
export interface ISchedulerResource {
    /** The person identifier. */
    PersonId: number,

    /** The scheduled attendance confirmation status of the resource. */
    ConfirmationStatus: string,

    /** The group member ID. NOTE: This will be NULL if the resource list has manually added personIds and/or comes from a Person DataView. */
    GroupMemberId?: number | null,

    /** The nickname of the person. */
    PersonNickName?: string | null,

    /** The last name of the person. */
    PersonLastName?: string | null,

    /** The name of the person. */
    PersonName?: string | null,

    /** The photo URL for the person. */
    PersonPhotoUrl?: string | null,

    /** The last attendance date time. */
    LastAttendanceDateTime?: string | null,

    /** The last attendance date time, formattted. */
    LastAttendanceDateTimeFormatted?: string | null,

    /** The note. */
    Note?: string | null,

    /** Whether this person has a conflict. */
    HasConflict: boolean,

    /** The conflict note. */
    ConflictNote?: string | null,

    /** Whether this Person has blackout conflict for all the occurrences. */
    HasBlackoutConflict: boolean,

    /** Whether this Person has partial blackout conflict (blackout for some of the occurrences, but not all of them). */
    HasPartialBlackoutConflict: boolean,

    /** The number of days shown in the Group Scheduler. */
    DisplayedDaysCount?: number | null,

    /** Obsolete: Use DisplayedDaysCount instead */
    OccurrenceDateCount: number,

    /** The displayed time slot count. */
    DisplayedTimeSlotCount?: number | null,

    /** The blackout dates */
    BlackoutDates?: string[] | null,

    /** Whether this Person has group requirements conflict. */
    HasGroupRequirementsConflict: boolean,

    /** Whether this Person has scheduling conflict with some other group for this schedule+date. */
    HasSchedulingConflict: boolean,

    /** The scheduling conflicts. */
    SchedulingConflicts?: ISchedulerResourceAssignment[] | null,

    /** Whether this Person is already scheduled for this group+schedule+date. */
    IsAlreadyScheduledForGroup?: boolean | null,

    // GroupRole,

    /** The name of the group role. */
    GroupRoleName?: string | null,

    /** The resource's preferences. */
    ResourcePreferenceList?: ISchedulerResourceAssignment[] | null,

    /** Teh resource's scheduled list. */
    ResourceScheduledList?: ISchedulerResourceAssignment[] | null
}

/**
 * A scheduler resource (Person) that has been associated with an attendance occurrence in some sort of scheduled state (Pending, Confirmed or Declined).
 * Representation of: https://github.com/SparkDevNetwork/Rock/blob/8dfb45edcbf4f166d483f6e96ed39806f3ca6a1b/Rock/Model/Event/Attendance/AttendanceService.cs#L3045
 */
export interface ISchedulerResourceAttend extends ISchedulerResource {
    /** The attendance identifier. */
    AttendanceId: number,

    /** The occurrence date. */
    OccurrenceDate?: string | null,

    /** How the scheduled attendance instance matches the preference of the individual. */
    MatchesPreference: string,

    /** Whether this scheduled resource has a blackout conflict for the occurrence date. */
    HasBlackoutConflict: boolean,

    /** The declined reason. */
    DeclinedReason?: string | null
}

/**
 * Information about a group scheduler occurrence's progress (towards filling the specified min, desired and max capcacities).
 */
export interface IScheduleProgress {
    /** The minimum capacity for this occurrence. */
    minimumCapacity?: number | null,

    /** The desired capacity for this occurrence. */
    desiredCapacity?: number | null,

    /** The maximum capacity for this occurrence. */
    maximumCapacity?: number | null,

    /** The count of confirmed resources for this occurrence. */
    confirmedCount: number,

    /** The count of pending resources for this occurrence. */
    pendingCount: number
}

/**
 * The available progress states for a group scheduler occurrence.
 */
export const ProgressState = {
    danger: "danger",
    critical: "critical",
    warning: "warning",
    success: "success"
};

/**
 * The actions that can be taken for a given, scheduled resource.
 */
export enum ResourceAction {
    MarkConfirmed = 0,
    MarkPending = 1,
    MarkDeclined = 2,
    ResendConfirmation = 3,
    UpdatePreference = 4,
    Remove = 5
}