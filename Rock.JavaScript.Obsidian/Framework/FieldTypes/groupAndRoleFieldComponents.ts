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
import { computed, defineComponent, ref, watch } from "vue";
import { getFieldEditorProps, getFieldConfigurationProps } from "./utils";
import GroupAndRolePicker from "@Obsidian/Controls/groupAndRolePicker.obs";
import TextBox from "@Obsidian/Controls/textBox.obs";
import { ConfigurationValueKey, GroupAndRoleValue } from "./groupAndRoleField.partial";
import { ListItemBag } from "@Obsidian/ViewModels/Utility/listItemBag";


export const EditComponent = defineComponent({
    name: "GroupAndRoleField.Edit",

    components: {
        GroupAndRolePicker
    },

    props: getFieldEditorProps(),

    setup(props, { emit }) {
        const group = ref({} as ListItemBag);
        const groupType = ref({} as ListItemBag);
        const groupRoleValue = ref({} as ListItemBag);

        watch(() => props.modelValue, () => {
            if (props.modelValue) {
                const internalValue = JSON.parse(props.modelValue || "{}") as GroupAndRoleValue;
                group.value = internalValue.group;
                groupType.value = internalValue.groupType;
                groupRoleValue.value = internalValue.groupRole;
            }
        }, { immediate: true });

        const groupLabel = computed((): string => {
            return props.configurationValues[ConfigurationValueKey.GroupRolePickerLabel] ?? "Group";
        });

        watch(() => [group.value, groupType.value, groupRoleValue], () => {

            const newValue = {
                groupType: groupType.value,
                group: group.value,
                groupRole: groupRoleValue.value
            };
            emit("update:modelValue", JSON.stringify(newValue));
        }, { deep: true });

        return {
            group,
            groupType,
            groupRoleValue,
            groupLabel
        };
    },

    template: `
    <div>
        <GroupAndRolePicker v-model="groupRoleValue" :groupLabel="groupLabel" v-model:groupType="groupType" v-model:group="group" />
    </div>
`
});

export const ConfigurationComponent = defineComponent({
    name: "GroupAndRoleField.Configuration",

    components: {
        TextBox
    },

    props: getFieldConfigurationProps(),

    emits: [
        "update:modelValue",
        "updateConfigurationValue"
    ],

    setup(props, { emit }) {
        const groupAndRolePickerLabel = ref("Group");

        /**
         * Update the modelValue property if any value of the dictionary has
         * actually changed. This helps prevent unwanted postbacks if the value
         * didn't really change - which can happen if multiple values get updated
         * at the same time.
         *
         * @returns true if a new modelValue was emitted to the parent component.
         */
        const maybeUpdateModelValue = (): boolean => {
            const newValue: Record<string, string> = {};

            // Construct the new value that will be emitted if it is different
            // than the current value.
            newValue[ConfigurationValueKey.GroupRolePickerLabel] = groupAndRolePickerLabel.value ?? "";

            const anyValueChanged = newValue[ConfigurationValueKey.GroupRolePickerLabel] !== props.modelValue[ConfigurationValueKey.GroupRolePickerLabel];

            // If any value changed then emit the new model value.
            if (anyValueChanged) {
                emit("update:modelValue", newValue);
                return true;
            }
            else {
                return false;
            }
        };

        /**
        * Emits the updateConfigurationValue if the value has actually changed.
        *
        * @param key The key that was possibly modified.
        * @param value The new value.
        */

        const maybeUpdateConfiguration = (key: string, value: string): void => {
            if (maybeUpdateModelValue()) {
                emit("updateConfigurationValue", key, value);
            }
        };

        // Watch for changes coming in from the parent component and update our
        // data to match the new information.
        watch(() => [props.modelValue, props.configurationProperties], () => {
            groupAndRolePickerLabel.value = props.modelValue[ConfigurationValueKey.GroupRolePickerLabel] ?? "Group";
        }, {
            immediate: true
        });

        watch(groupAndRolePickerLabel, val => maybeUpdateConfiguration(ConfigurationValueKey.GroupRolePickerLabel, val ?? ""));

        return {
            groupAndRolePickerLabel
        };
    },

    template: `
    <TextBox label="Group/Role Picker Label" v-model="groupAndRolePickerLabel" help="The label for the group/role picker." />
    `
});
