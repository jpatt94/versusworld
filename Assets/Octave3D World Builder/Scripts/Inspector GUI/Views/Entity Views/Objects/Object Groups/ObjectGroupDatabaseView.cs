#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace O3DWB
{
    [Serializable]
    public class ObjectGroupDatabaseView : EntityView
    {
        #region Private Variables
        [NonSerialized]
        private ObjectGroupDatabase _database;

        [SerializeField]
        private ObjectGroupDatabaseViewData _viewData;
        #endregion

        #region Private Properties
        private ObjectGroupDatabaseViewData ViewData
        {
            get
            {
                if (_viewData == null) _viewData = Octave3DWorldBuilder.ActiveInstance.CreateScriptableObject<ObjectGroupDatabaseViewData>();
                return _viewData;
            }
        }
        #endregion

        #region Constructors
        public ObjectGroupDatabaseView(ObjectGroupDatabase database)
        {
            _database = database;
        }
        #endregion

        #region Protected Methods
        protected override void RenderContent()
        {
            if (_database.IsEmpty) EditorGUILayoutEx.InformativeLabel("There are no object groups currently available.");
            else
            {
                RenderActiveGroupSelectionPopup();
                _database.ActiveGroup.View.Render();
            }

            RenderActionControls();
            RenderPreserveGroupChildrenToggle();
        }
        #endregion

        #region Private Methods
        private void RenderActiveGroupSelectionPopup()
        {
            int newInt = EditorGUILayoutEx.Popup(GetContentForActiveGroupSelectionPopup(), _database.IndexOfActiveGroup, _database.GetAllObjectGroupNames());
            if(newInt != _database.IndexOfActiveGroup)
            {
                UndoEx.RecordForToolAction(_database);
                _database.SetActiveObjectGroup(_database.GetObjectGroupByIndex(newInt));
            }
        }

        private GUIContent GetContentForActiveGroupSelectionPopup()
        {
            var content = new GUIContent();
            content.text = "Active group";
            content.tooltip = "Allows you to change the active object group.";

            return content;
        }

        private void RenderActionControls()
        {
            EditorGUILayout.BeginHorizontal();
            RenderCreateNewGroupButton();
            RenderCreateNewGroupNameChangeTextField();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            RenderRemoveActiveGroupButton();
            RenderRemoveAllGroupsButton();
            EditorGUILayout.EndHorizontal();

            if(_database.ActiveGroup != null)
            {
                EditorGUILayout.BeginHorizontal();
                RenderMakeActiveGroupStaticButton();
                RenderMakeActiveGroupDynamicButton();
                EditorGUILayout.EndHorizontal();
            }
        }

        private void RenderCreateNewGroupButton()
        {
            if (GUILayout.Button(GetContentForCreateNewGroupButton(), GUILayout.Width(EditorGUILayoutEx.PreferedActionButtonWidth)))
            {
                UndoEx.RecordForToolAction(_database);
                _database.CreateObjectGroup(ViewData.NameForNewGroup);
            }
        }

        private GUIContent GetContentForCreateNewGroupButton()
        {
            var content = new GUIContent();
            content.text = "Create new group";
            content.tooltip = "Creates new object groups using the name specified in the adjacent text field.";

            return content;
        }

        private void RenderCreateNewGroupNameChangeTextField()
        {
            string newString = EditorGUILayout.TextField(ViewData.NameForNewGroup);
            if (newString != ViewData.NameForNewGroup)
            {
                UndoEx.RecordForToolAction(ViewData);
                ViewData.NameForNewGroup = newString;
            }
        }

        private void RenderRemoveActiveGroupButton()
        {
            if(GUILayout.Button(GetContentForRemoveActiveGroupButton(), GUILayout.Width(EditorGUILayoutEx.PreferedActionButtonWidth)))
            {
                UndoEx.RecordForToolAction(_database);
                _database.RemoveAndDestroyObjectGroup(_database.ActiveGroup);
            }
        }

        private GUIContent GetContentForRemoveActiveGroupButton()
        {
            var content = new GUIContent();
            content.text = "Remove active group";
            content.tooltip = "Removes the active group.";

            return content;
        }

        private void RenderRemoveAllGroupsButton()
        {
            if(GUILayout.Button(GetContentForRemoveAllGroupsButton()))
            {
                UndoEx.RecordForToolAction(_database);
                _database.RemoveAndDestroyAllObjectGroups();
            }
        }

        private GUIContent GetContentForRemoveAllGroupsButton()
        {
            var content = new GUIContent();
            content.text = "Remove all groups";
            content.tooltip = "Removes all object groups.";

            return content;
        }

        private void RenderPreserveGroupChildrenToggle()
        {
            bool newBool = EditorGUILayout.ToggleLeft(GetContentForPreserveGroupChildrenToggle(), _database.PreserveGroupChildren);
            if(newBool != _database.PreserveGroupChildren)
            {
                UndoEx.RecordForToolAction(_database);
                _database.PreserveGroupChildren = newBool;
            }
        }

        private GUIContent GetContentForPreserveGroupChildrenToggle()
        {
            var content = new GUIContent();
            content.text = "Preserve group children";
            content.tooltip = "If this is checked, when a group is deleted, its children will be moved one level up the hierarchy so that they don't get deleted together with the group's parent.";

            return content;
        }

        private void RenderMakeActiveGroupStaticButton()
        {
            if (GUILayout.Button(GetContentForMakeActiveGroupStaticButton(), GUILayout.Width(EditorGUILayoutEx.PreferedActionButtonWidth)))
            {
                List<GameObject> allObjectsInActivegroup = _database.ActiveGroup.GroupObject.GetAllChildrenIncludingSelf();
                UndoEx.RecordForToolAction(allObjectsInActivegroup);
                ObjectActions.MakeObjectsStatic(allObjectsInActivegroup);
            }
        }

        private GUIContent GetContentForMakeActiveGroupStaticButton()
        {
            var content = new GUIContent();
            content.text = "Make active group static";
            content.tooltip = "Marks the active group (and all its child objects) as static.";

            return content;
        }

        private void RenderMakeActiveGroupDynamicButton()
        {
            if (GUILayout.Button(GetContentForMakeActiveGroupDynamicButton()))
            {
                List<GameObject> allObjectsInActivegroup = _database.ActiveGroup.GroupObject.GetAllChildrenIncludingSelf();
                UndoEx.RecordForToolAction(allObjectsInActivegroup);
                ObjectActions.MakeObjectsDynamic(allObjectsInActivegroup);
            }
        }

        private GUIContent GetContentForMakeActiveGroupDynamicButton()
        {
            var content = new GUIContent();
            content.text = "Make active group dynamic";
            content.tooltip = "Marks the active group (and all its child objects) as dynamic.";

            return content;
        }
        #endregion
    }
}
#endif