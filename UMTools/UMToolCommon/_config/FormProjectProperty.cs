using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UMTools.Common;

namespace UMTools.Common
{
	public partial class FormProjectProperty : Form
	{
		const string REG_SUB_KEY = "UMToolCommon";
		const string REG_KEY_LAST_TAB = "property_tab";

		public static DialogResult Open()
		{
			using( FormProjectProperty form = new FormProjectProperty() )
			{
				return form.ShowDialog();
			}
		}

		ProjectPropertyConfig mProjectProperty = null;
		ProjectGlobaltypePropertyConfig mProjectGlobaltypeProperty = null;
		GlobalPropertyConfig mGlobalProperty = null;

		public FormProjectProperty()
		{
			InitializeComponent();
			this.DialogResult = DialogResult.Cancel;

			mGlobalProperty = ProjectConfig.Instance.GlobalProperty;
			mProjectProperty = ProjectConfig.Instance.CurrentProjectProerty;
			mProjectGlobaltypeProperty = ProjectConfig.Instance.CurrentProjectGlobaltypeProperty;

			pg_global.SelectedObject = mGlobalProperty;

			string last_tab = ToolUtil.GetPrefs<string>( REG_SUB_KEY, REG_KEY_LAST_TAB, "" );
			foreach( TabPage page in tab_control.TabPages )
			{
				if( page.Name == last_tab )
				{
					tab_control.SelectedTab = page;
					break;
				}
			}

			List<ProjectConfig.Data> data_list = ProjectConfig.Instance.DataList;
			cb_projectselect.Items.Clear();
			foreach( ProjectConfig.Data data in data_list )
			{
				cb_projectselect.Items.Add( data.ProjectName );
			}

			string last_selected_project = mProjectProperty.ProjectName;
			cb_projectselect.SelectedItem = last_selected_project;
		}

		//------------------------------------------------------------------------
		private void cb_projectselect_SelectedIndexChanged( object sender, EventArgs e )
		{
			string selected_project = cb_projectselect.SelectedItem.ToString();

			ProjectConfig.Data config_data = ProjectConfig.Instance.FindProject( selected_project );

			this.BackColor = ToolUtil.GetColorFromHEX( config_data.FormColor );
			string last_global_type = config_data.GlobalTypeList[0];

			cb_globaltype.Items.Clear();
			foreach(string g_type in config_data.GlobalTypeList)
			{
				cb_globaltype.Items.Add( g_type );					
			}

			cb_globaltype.SelectedItem = last_global_type;			
		}

		private void FormProjectConfig_FormClosed( object sender, FormClosedEventArgs e )
		{
			GlobalPropertyConfig.Save( mGlobalProperty );
			ProjectPropertyConfig.Save( mProjectProperty );
			ProjectGlobaltypePropertyConfig.Save( mProjectGlobaltypeProperty );
		}

		private void cb_globaltype_SelectedIndexChanged( object sender, EventArgs e )
		{
			if( cb_globaltype.SelectedItem == null )
				return;

			string selected_project = cb_projectselect.SelectedItem.ToString();
			string selected_globaltype = cb_globaltype.SelectedItem.ToString();

			GlobalPropertyConfig.Save( mGlobalProperty );
			ProjectPropertyConfig.Save( mProjectProperty );
			ProjectGlobaltypePropertyConfig.Save( mProjectGlobaltypeProperty );

			mProjectProperty = ProjectPropertyConfig.Load( selected_project );
			mProjectGlobaltypeProperty = ProjectGlobaltypePropertyConfig.Load( selected_project, selected_globaltype );

			pg_project.SelectedObject = mProjectProperty;
			pg_project_globaltype.SelectedObject = mProjectGlobaltypeProperty;
		}

		private void btn_apply_Click( object sender, EventArgs e )
		{
			ProjectConfig.Instance.ApplyProject( mProjectProperty, mProjectGlobaltypeProperty );
			this.DialogResult = DialogResult.OK;
			Close();
		}

		private void tab_control_SelectedIndexChanged( object sender, EventArgs e )
		{
			string curr_tab_name = tab_control.SelectedTab.Name;
			ToolUtil.SavePrefs( REG_SUB_KEY, REG_KEY_LAST_TAB, curr_tab_name );
		}
	}
}
