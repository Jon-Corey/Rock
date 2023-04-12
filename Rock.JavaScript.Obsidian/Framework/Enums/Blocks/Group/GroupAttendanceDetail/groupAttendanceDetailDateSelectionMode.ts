//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the Rock.CodeGeneration project
//     Changes to this file will be lost when the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
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

/** The date selection mode options available in the Group Attendance Detail block. */
export const GroupAttendanceDetailDateSelectionMode = {
    /** No date selection will be available. */
    None: 0,

    /** A read-only date will be presented to the individual. */
    Readonly: 1,

    /** A date picker will be presented to the individual. */
    DatePicker: 2,

    /** A scheduled date (occurrence date and schedule) picker will be presented to the individual. */
    ScheduledDatePicker: 3
} as const;

/** The date selection mode options available in the Group Attendance Detail block. */
export const GroupAttendanceDetailDateSelectionModeDescription: Record<number, string> = {
    0: "None",

    1: "Readonly",

    2: "Date Picker",

    3: "Scheduled Date Picker"
};

/** The date selection mode options available in the Group Attendance Detail block. */
export type GroupAttendanceDetailDateSelectionMode = typeof GroupAttendanceDetailDateSelectionMode[keyof typeof GroupAttendanceDetailDateSelectionMode];