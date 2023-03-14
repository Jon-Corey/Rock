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

import { Component, PropType, VNode } from "vue";

/** A function that will be called in response to an action. */
export type GridActionCallback = (event: Event) => void | Promise<void>;

/** Defines a single action related to a Grid control. */
export type GridAction = {
    /**
     * The title of the action, this should be a very short (one or two words)
     * description of the action that will be performed, such as "Delete".
     */
    title?: string;

    /**
     * The tooltip to display for this action.
     */
    tooltip?: string;

    /**
     * The CSS class for the icon used when displaying this action.
     */
    iconCssClass?: string;

    /** The callback function that will handle the action. */
    handler?: GridActionCallback;

    /** If true then the action will be disabled and not respond to clicks. */
    disabled?: boolean;

    /** True if the action is currently executing. */
    executing: boolean;
};

export type GridColumnDefinition = {
    /** The unique name of this column. */
    name: string;

    /** The title to display in the column header. */
    title?: string;

    /** The name of the field in the row object. */
    field?: string;

    /**
     * Formats the value for display in the cell. Should return HTML safe
     * content, meaning if you intend to display the < character you need
     * to HTML encode it as &lt;.
     */
    format: VNode | Component;

    /** Gets the value to use when filtering on the quick filter. */
    quickFilterValue: (row: Record<string, unknown>, column: GridColumnDefinition) => string | undefined;

    /**
     * Gets the unique value representation of the cell value. For example,
     * a Person column might return the person Guid.
     */
    uniqueValue: ValueFormatterFunction;

    /** Gets the value to use when sorting. */
    sortValue?: ValueFormatterFunction;

    /** Gets the value to use when filtering. */
    filterValue: (row: Record<string, unknown>, column: GridColumnDefinition) => unknown | undefined;

    filter?: IGridColumnFilter;

    props: Record<string, unknown>;
};

export type ValueFormatterFunction = (row: Record<string, unknown>, column: GridColumnDefinition) => string | number | undefined;

export interface IGridColumnFilter {
    component: Component;

    matches: (needle: unknown, haystack: unknown, column: GridColumnDefinition) => boolean;
}

export type FilterComponentProps = {
    modelValue: {
        type: PropType<unknown>,
        required: false
    },

    column: {
        type: PropType<GridColumnDefinition>,
        required: true
    },

    rows: {
        type: PropType<Record<string, unknown>[]>,
        required: true
    }
};
