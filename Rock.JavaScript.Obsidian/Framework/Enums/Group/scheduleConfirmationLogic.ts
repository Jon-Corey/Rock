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

/** Schedule Confirmation Logic */
export const ScheduleConfirmationLogic = {
    /** The ask */
    Ask: 0,

    /** The auto accept */
    AutoAccept: 1
} as const;

/** Schedule Confirmation Logic */
export const ScheduleConfirmationLogicDescription: Record<number, string> = {
    0: "Ask",

    1: "Auto Accept"
};

/** Schedule Confirmation Logic */
export type ScheduleConfirmationLogic = typeof ScheduleConfirmationLogic[keyof typeof ScheduleConfirmationLogic];