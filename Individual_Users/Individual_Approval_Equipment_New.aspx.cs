﻿using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Security.Cryptography;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Net.Mail;
using System.IO;
using System.Web.Configuration;
using System.Collections.Specialized;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Globalization;

public partial class Individual_Users_Individual_Approval_Equipment_New : System.Web.UI.Page
{
    Common_Class CC = new Common_Class();
    public string Email_User_ID = System.Configuration.ConfigurationManager.AppSettings["EmailID"];
    public string Email_Password = System.Configuration.ConfigurationManager.AppSettings["EmailPassword"];
    SqlConnection con = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ToString());
    SqlCommand logincommand1;
    SqlDataReader objreader1;
    protected decimal available_balance_amount = 0;
    protected decimal items_total_amount;
    protected decimal items_total_amount_p;
    private Random random = new Random();

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["Email"] == null)
        {
            Response.Redirect("../SessionExpire.aspx");
        }
        Page.MaintainScrollPositionOnPostBack = true;
        if (!Page.IsPostBack)
        {
            //'Disable Submit Button
            //Me.btn_submit.Attributes.Add("onclick", DisableTheButton(Me.Page, Me.btn_submit))

            GridView1.Style.Add("display", "none");
            SetInitialRow();
            string reference_no = "";

            try
            {
                reference_no = Request.QueryString["Session_Id"].ToString();
            }
            catch (Exception ex)
            {
                Response.Redirect("Invalid_Request.aspx");
            }

            if (Session["Form_A_Reference_No"] == reference_no)
                Response.Redirect("Invalid_Request.aspx");


            string financial_year = Session["Form_A_Entry_Financial_Year"].ToString();
            string department_division_id = Session["Department_Divison_Id"].ToString();
            //' Dim budget_main_head  = Session["Form_A_Entry_Main_Head"];
            string budget_head = Session["Form_A_Entry_Head"].ToString();
            string budget_sub_head = Session["Form_A_Entry_Sub_Head"].ToString();

            string budget_main_head_for_balance = Session["Form_A_Entry_Main_Head"].ToString();
            string budget_head_for_balance = Session["Form_A_Entry_Head"].ToString();
            string budget_sub_head_for_balance = Session["Form_A_Entry_Sub_Head"].ToString();


            lbl_current_date.Text = DateTime.Now.ToString("dd/MMM/yyyy");


            // 'Get Department Name from the database
            SqlDataReader objreader1;
            logincommand1 = new SqlCommand();
            logincommand1.CommandText = "SELECT Department_Name FROM Department_Division_Master  WHERE Department_Id = '" + department_division_id.ToString() + "'";
            logincommand1.Connection = con; ;
            con.Open(); ;
            objreader1 = logincommand1.ExecuteReader();
            if (objreader1.Read())
                lbl_department_name.Text = objreader1["Department_Name"].ToString();

            con.Close(); ;

            // 'Get Budget Head Description
            string budget_head_description = "";
            SqlDataReader objreader2;
            SqlCommand logincommand2 = new SqlCommand();
            logincommand2.CommandText = "SELECT Project_Title FROM Budget_Master_Individual WHERE Financial_Year = '" + financial_year.ToString() + "' AND Email = '" + Session["Email"].ToString() + "' AND  Head = '" + budget_head.ToString() + "' AND Sub_Head = '" + budget_sub_head.ToString() + "'";
            logincommand2.Connection = con; ;
            con.Open(); ;
            objreader2 = logincommand2.ExecuteReader();
            if (objreader2.Read())
                budget_head_description = objreader2["Project_Title"].ToString();

            con.Close(); ;

            lbl_budget_head_details.Text = budget_head.ToString() + "/" + budget_sub_head.ToString();
            lbl_Project_head_Title.Text = budget_head_description.ToString();

            if (!Page.IsPostBack)
            {

                // 'Get Justification Record Count
                SqlCommand command5;
                int approval_justification_record_count;
                //' Dim str5 As String = "SELECT COUNT(*) FROM Form_A_Justification_Details_Draft WHERE Reference_No = '" + reference_no.ToString() + "' "
                string str5 = "SELECT COUNT(*) FROM Form_A_Equipment_Justification_Details_Draft WHERE Reference_No = '" + reference_no.ToString() + "' ";
                command5 = new SqlCommand(str5, con);
                con.Open(); ;
                approval_justification_record_count = Convert.ToInt32(command5.ExecuteScalar());
                con.Close(); ;

                // 'Get Justification details from the database
                if (approval_justification_record_count > 0)
                {

                    SqlDataReader objreader6;
                    SqlCommand logincommand6 = new SqlCommand();
                    // 'logincommand6.CommandText = "SELECT Justification_Details FROM Form_A_Justification_Details_Draft WHERE Reference_No = '" + reference_no.ToString() + "' "
                    logincommand6.CommandText = "SELECT Justification_Details FROM Form_A_Equipment_Justification_Details_Draft WHERE Reference_No = '" + reference_no.ToString() + "' ";
                    logincommand6.Connection = con; ;
                    con.Open(); ;
                    objreader6 = logincommand6.ExecuteReader();
                    if (objreader6.Read())
                        txt_justification.Text = objreader6["Justification_Details"].ToString();

                    con.Close(); ;
                }


                // 'Get Items Record Count
                SqlCommand command7;
                int approval_items_record_count;
                // ' Dim str7 As String = "SELECT COUNT(*) FROM Form_A_Item_Details_Draft WHERE Reference_No = '" + reference_no.ToString() + "' "
                string str7 = "SELECT COUNT(*) FROM Form_A_Equipment_Item_Details_Draft WHERE Reference_No = '" + reference_no.ToString() + "' ";
                command7 = new SqlCommand(str7, con);
                con.Open(); ;
                approval_items_record_count = Convert.ToInt32(command7.ExecuteScalar());
                con.Close(); ;

                if (approval_items_record_count > 0)
                {
                    //'Bind Data to Gridview
                    Bind_Item_Details();
                    div_item_details.Visible = true;
                }
            }


            // '----------------Get Avalaible Balance-----
            SqlCommand command8;
            int institute_approval_record_count;
            // 'Dim str8 As String = "SELECT COUNT(*) FROM Institute_Approval_Master WHERE Financial_Year = '" + financial_year.ToString() + "' AND Department_Id = '" + department_division_id.ToString() + "' AND Main_Head = '" + budget_main_head_for_balance.ToString() + "' AND Head = '" + budget_head_for_balance.ToString() + "' AND Sub_Head = '" + budget_sub_head_for_balance.ToString() + "' AND Approval_Status IN ('Approved') "
            string str8 = "SELECT COUNT(*) FROM tbl_Individual_Consumables_form WHERE Financial_Year = '" + financial_year.ToString() + "' AND Department_Id = '" + department_division_id.ToString() + "' AND   Head = '" + budget_head_for_balance.ToString() + "' AND Sub_Head = '" + budget_sub_head_for_balance.ToString() + "' AND Approval_Status IN ('Approved') ";
            command8 = new SqlCommand(str8, con);
            con.Open();
            institute_approval_record_count = Convert.ToInt32(command8.ExecuteScalar());
            con.Close();



            decimal approved_amount = 0;
            decimal spent_amount = 0;
            decimal balance_amount = 0;

            if (institute_approval_record_count == 0)
            {

                //  'If No Approval is Submitted then Get the Approved Budget Head Amount from the database
                SqlDataReader objreader9;
                SqlCommand logincommand9 = new SqlCommand();
                //'logincommand9.CommandText = "SELECT Approved_Amount FROM Budget_Master WHERE Financial_Year = '" + financial_year.ToString() + "' AND Department_Id = '" + department_division_id.ToString() + "' AND Main_Head = '" + budget_main_head_for_balance.ToString() + "' AND Head = '" + budget_head_for_balance.ToString() + "' AND Sub_Head = '" + budget_sub_head_for_balance.ToString() + "' "
                logincommand9.CommandText = "SELECT Approved_Amount FROM Budget_Master_Individual WHERE Financial_Year = '" + financial_year.ToString() + "' AND Department_Id = '" + department_division_id.ToString() + "' AND   Head = '" + budget_head_for_balance.ToString() + "' AND Sub_Head = '" + budget_sub_head_for_balance.ToString() + "' ";
                logincommand9.Connection = con;
                con.Open();
                objreader9 = logincommand9.ExecuteReader();
                if (objreader9.Read())
                {
                    balance_amount = Convert.ToDecimal(objreader9["Approved_Amount"]);
                    available_balance_amount = balance_amount;


                    //    'lbl_budget_head_balance_amount.Text = String.Format("{0:C}", balance_amount)
                    lbl_budget_head_balance_amount.Text = "Rs." + " " + balance_amount.ToString().Replace(".00", "") + ".00" + "";
                }
                con.Close();
            }
            else if (institute_approval_record_count > 0)
            {
                decimal estimated_amount = 0;
                decimal bill_amount = 0;

                SqlDataReader objreader9;
                SqlCommand logincommand9 = new SqlCommand();
                //'logincommand9.CommandText = "SELECT Approved_Amount FROM Budget_Master WHERE Financial_Year = '" + financial_year.ToString() + "' AND Department_Id = '" + department_division_id.ToString() + "' AND Main_Head = '" + budget_main_head_for_balance.ToString() + "' AND Head = '" + budget_head_for_balance.ToString() + "' AND Sub_Head = '" + budget_sub_head_for_balance.ToString() + "' "
                logincommand9.CommandText = "SELECT Approved_Amount FROM Budget_Master_Individual WHERE Financial_Year = '" + financial_year.ToString() + "' AND Department_Id = '" + department_division_id.ToString() + "' AND Head = '" + budget_head_for_balance.ToString() + "' AND Sub_Head = '" + budget_sub_head_for_balance.ToString() + "' ";
                logincommand9.Connection = con;
                con.Open();
                objreader9 = logincommand9.ExecuteReader();
                if (objreader9.Read())
                    approved_amount = Convert.ToDecimal(objreader9["Approved_Amount"]);

                con.Close();


                // 'Estimated Amount
                SqlDataReader objreader10;
                SqlCommand logincommand10 = new SqlCommand();
                // ' logincommand10.CommandText = "SELECT CASE WHEN SUM(cast(Estimated_Amount as decimal(10,2))) is null then '0' ELSE SUM(cast(Estimated_Amount as decimal(10,2))) END as as Estimated_Amount FROM Institute_Approval_Master WHERE Financial_Year = '" + financial_year.ToString() + "' AND Department_Id = '" + department_division_id.ToString() + "' AND Main_Head = '" + budget_main_head_for_balance.ToString() + "' AND Head = '" + budget_head_for_balance.ToString() + "' AND Sub_Head = '" + budget_sub_head_for_balance.ToString() + "' AND Dean_Approval_Status IN ('Approved') AND Bill_Amount_Submit_Status = 'Pending'"
                logincommand10.CommandText = "SELECT CASE WHEN SUM(cast(Estimated_Amount as decimal(10,2))) is null then '0' ELSE SUM(cast(Estimated_Amount as decimal(10,2))) END as Estimated_Amount FROM tbl_Individual_Consumables_form WHERE Financial_Year = '" + financial_year.ToString() + "' AND Department_Id = '" + department_division_id.ToString() + "' AND  Head = '" + budget_head_for_balance.ToString() + "' AND Sub_Head = '" + budget_sub_head_for_balance.ToString() + "' AND Dean_Approval_Status IN ('Approved') AND Bill_Amount_Submit_Status = 'Pending'";
                logincommand10.Connection = con;
                con.Open();
                objreader10 = logincommand10.ExecuteReader();
                if (objreader10.Read())
                    estimated_amount = Convert.ToDecimal(objreader10["Estimated_Amount"].ToString());

                con.Close();


                //'Get Bill Amount/Actual Amount
                SqlDataReader objreader11;
                SqlCommand logincommand11 = new SqlCommand();
                //' logincommand11.CommandText = "SELECT CASE WHEN SUM(cast(Bill_Amount as decimal(10,2))) is null then '0' ELSE SUM(cast(Bill_Amount as decimal(10,2))) END as Billed_Amount FROM Institute_Approval_Master WHERE Financial_Year = '" + financial_year.ToString() + "' AND Department_Id = '" + department_division_id.ToString() + "' AND Main_Head = '" + budget_main_head_for_balance.ToString() + "' AND Head = '" + budget_head_for_balance.ToString() + "' AND Sub_Head = '" + budget_sub_head_for_balance.ToString() + "' AND Approval_Status IN ('Approved') AND Bill_Amount_Submit_Status = 'Processed' "
                logincommand11.CommandText = "SELECT CASE WHEN SUM(cast(Bill_Amount as decimal(10,2))) is null then '0' ELSE SUM(cast(Bill_Amount as decimal(10,2))) END as Billed_Amount FROM tbl_Individual_Consumables_form WHERE Financial_Year = '" + financial_year.ToString() + "' AND Department_Id = '" + department_division_id.ToString() + "' AND   Head = '" + budget_head_for_balance.ToString() + "' AND Sub_Head = '" + budget_sub_head_for_balance.ToString() + "' AND Approval_Status IN ('Approved') AND Bill_Amount_Submit_Status = 'Processed' ";
                logincommand11.Connection = con;
                con.Open();
                objreader11 = logincommand11.ExecuteReader();
                if (objreader11.Read())
                    bill_amount = Convert.ToDecimal(objreader11["Billed_Amount"].ToString());

                con.Close();

                balance_amount = approved_amount - (estimated_amount + bill_amount);
                available_balance_amount = balance_amount;
                lbl_budget_head_balance_amount.Text = "Rs." + " " + balance_amount.ToString().Replace(".00", "") + ".00" + "";

            }


            //'Get Head Name from the database
            string faculty_name = "";
            string Faculty_Salutation = "";

            SqlDataReader objreader12;
            SqlCommand logincommand12 = new SqlCommand();
            // 'logincommand12.CommandText = "SELECT Department_Head_Incharge_Name,Department_Head_Incharge_Salutation FROM Department_Division_Master WHERE Department_Id = '" + department_division_id.ToString() + "'"

            //logincommand12.CommandText = "SELECT  Faculty_Salutation, Faculty_Name FROM Faculty_Master WHERE Department_Id = '" + department_division_id.ToString() + "'";
            //logincommand12.Connection = con;
            //con.Open();
            //objreader12 = logincommand12.ExecuteReader();
            //if (objreader12.Read())
            //{
            //    Faculty_Salutation = objreader12["Faculty_Salutation"].ToString();
            //    faculty_name = objreader12["Faculty_Name"].ToString();

            //    Session["Head_FullName"] = faculty_name.ToString();

            //    // lbl_user_name.Text = faculty_name.ToString() + ", " + Faculty_Salutation.ToString();
            //    lbl_user_name.Text = Faculty_Salutation.ToString() + " " + faculty_name.ToString();
            //}

            logincommand12.CommandText = "SELECT  FullName FROM Login_Details_Individual WHERE Assigned_Department = '" + department_division_id.ToString() + "' and Username='" + Session["Email"].ToString() + "'";
            logincommand12.Connection = con;
            con.Open();
            objreader12 = logincommand12.ExecuteReader();
            if (objreader12.Read())
            {
                faculty_name = objreader12["FullName"].ToString();
                Session["Head_FullName"] = faculty_name.ToString();
                lbl_user_name.Text = faculty_name.ToString();
            }

            con.Close();


            // 'Get Intitute Approval Creation Permission Details from the database

            string institute_approval_creation_permission_status = "";
            SqlDataReader objreader13;
            SqlCommand logincommand13 = new SqlCommand();
            logincommand13.CommandText = "SELECT Institute_Approval_Permission_Status FROM Institute_Approval_Permission_Details";
            logincommand13.Connection = con;
            con.Open();
            objreader13 = logincommand13.ExecuteReader();
            if (objreader13.Read())
            {
                institute_approval_creation_permission_status = objreader13["Institute_Approval_Permission_Status"].ToString();

                if (institute_approval_creation_permission_status == "No")
                    Response.Redirect("Institute_Approval_Creation_Permission_Denied.aspx");

            }
            con.Close();


            if (Page.IsPostBack == false)
                FillCapctha();/* TODO ERROR: Skipped SkippedTokensTrivia */
        }
    }

    private void FillCapctha()
    {
        try
        {
            Random random = new Random();
            // Dim combination As String = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz"
            string combination = "123456789";
            StringBuilder captcha = new StringBuilder();
            for (int i = 0; i <= 3; i++)
                captcha.Append(combination[random.Next(combination.Length)]);
            Session["captcha"] = captcha.ToString();
            imgCaptcha.ImageUrl = "Captcha/Captcha.aspx?" + DateTime.Now.Ticks.ToString();
        }
        catch
        {
            throw;
        }
    }


    private void Bind_Item_Details()
    {
        string reference_no;
        reference_no = Request.QueryString["Session_Id"].ToString();

        // SqlConnection constr = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ToString());
        using (SqlConnection con = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ToString()))
        {
            // Dim str1 As String = "SELECT * FROM Form_A_Item_Details_Draft WHERE Reference_No = '" + reference_no.ToString() + "' "
            string str1 = "SELECT * FROM Form_A_Equipment_Item_Details_Draft WHERE Reference_No = '" + reference_no.ToString() + "' ";
            using (SqlCommand cmd = new SqlCommand(str1))
            {
                using (SqlDataAdapter sda = new SqlDataAdapter())
                {
                    cmd.Connection = con; ;
                    sda.SelectCommand = cmd;
                    using (DataTable dt = new DataTable())
                    {
                        sda.Fill(dt);
                        DG1.DataSource = dt;
                        DG1.DataBind();
                    }
                }
            }
        }
    }


    #region Add New ROW
    protected void grd_Travel_details_RowDeleting(object sender, GridViewDeleteEventArgs e)
    {
        SetRowData();
        if (ViewState["CurrentTable"] != null)
        {
            DataTable dt = (DataTable)ViewState["CurrentTable"];
            DataRow drCurrentRow = null;
            int rowIndex = Convert.ToInt32(e.RowIndex);
            if (dt.Rows.Count > 1)
            {
                dt.Rows.Remove(dt.Rows[rowIndex]);
                drCurrentRow = dt.NewRow();
                ViewState["CurrentTable"] = dt;
                grd_Travel_details.DataSource = dt;
                grd_Travel_details.DataBind();

                for (int i = 0; i < grd_Travel_details.Rows.Count - 1; i++)
                {
                    grd_Travel_details.Rows[i].Cells[0].Text = Convert.ToString(i + 1);
                }
                SetPreviousData();

            }
            else
            {
                SetInitialRow();
            }
        }
    }
    protected void grd_Travel_details_RowDataBound(object sender, GridViewRowEventArgs e)
    {

    }



    protected void ButtonAdd_Click(object sender, EventArgs e)
    {
        AddNewRowToGrid();
    }

    private void SetInitialRow()
    {

        DataTable dt = new DataTable();
        DataRow dr = null;
        dt.Columns.Add(new DataColumn("RowNumber", typeof(string)));
        dt.Columns.Add(new DataColumn("txt_item_description", typeof(string)));
        dt.Columns.Add(new DataColumn("txt_item_quantity", typeof(string)));
        dt.Columns.Add(new DataColumn("txt_item_cost", typeof(string)));

        dr = dt.NewRow();
        dr["RowNumber"] = 1;
        dr["txt_item_description"] = string.Empty;
        dr["txt_item_quantity"] = string.Empty;
        dr["txt_item_cost"] = string.Empty;


        dt.Rows.Add(dr);

        //Store the DataTable in ViewState
        ViewState["CurrentTable"] = dt;

        grd_Travel_details.DataSource = dt;
        grd_Travel_details.DataBind();
    }

    private void AddNewRowToGrid()
    {
        int rowIndex = 0;

        DataTable dtCurrentTable = (DataTable)ViewState["CurrentTable"];
        DataRow drCurrentRow = null;
        if (dtCurrentTable.Rows.Count > 0)
        {
            for (int i = 1; i <= dtCurrentTable.Rows.Count; i++)
            {
                TextBox txt_item_description = (TextBox)grd_Travel_details.Rows[rowIndex].FindControl("txt_item_description");
                TextBox txt_item_quantity = (TextBox)grd_Travel_details.Rows[rowIndex].FindControl("txt_item_quantity");
                TextBox txt_item_cost = (TextBox)grd_Travel_details.Rows[rowIndex].FindControl("txt_item_cost");


                drCurrentRow = dtCurrentTable.NewRow();
                drCurrentRow["RowNumber"] = i + 1;

                dtCurrentTable.Rows[i - 1]["txt_item_description"] = txt_item_description.Text;
                dtCurrentTable.Rows[i - 1]["txt_item_quantity"] = txt_item_quantity.Text;
                dtCurrentTable.Rows[i - 1]["txt_item_cost"] = txt_item_cost.Text;

                rowIndex++;
            }
            dtCurrentTable.Rows.Add(drCurrentRow);
            ViewState["CurrentTable"] = dtCurrentTable;

            grd_Travel_details.DataSource = dtCurrentTable;
            grd_Travel_details.DataBind();
        }

        SetPreviousData();
    }

    private void SetPreviousData()
    {
        int rowIndex = 0;


        DataTable dt = (DataTable)ViewState["CurrentTable"];
        if (dt.Rows.Count > 0)
        {
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                TextBox txt_item_description = (TextBox)grd_Travel_details.Rows[rowIndex].FindControl("txt_item_description");
                TextBox txt_item_quantity = (TextBox)grd_Travel_details.Rows[rowIndex].FindControl("txt_item_quantity");
                TextBox txt_item_cost = (TextBox)grd_Travel_details.Rows[rowIndex].FindControl("txt_item_cost");


                txt_item_description.Text = dt.Rows[i]["txt_item_description"].ToString();
                txt_item_quantity.Text = dt.Rows[i]["txt_item_quantity"].ToString();
                txt_item_cost.Text = dt.Rows[i]["txt_item_cost"].ToString();


                rowIndex++;
            }
        }
    }

    private void SetRowData()
    {
        int rowIndex = 0;

        if (ViewState["CurrentTable"] != null)
        {
            DataTable dtCurrentTable = (DataTable)ViewState["CurrentTable"];
            DataRow drCurrentRow = null;
            if (dtCurrentTable.Rows.Count > 0)
            {
                for (int i = 1; i <= dtCurrentTable.Rows.Count; i++)
                {
                    TextBox txt_item_description = (TextBox)grd_Travel_details.Rows[rowIndex].FindControl("txt_item_description");
                    TextBox txt_item_quantity = (TextBox)grd_Travel_details.Rows[rowIndex].FindControl("txt_item_quantity");
                    TextBox txt_item_cost = (TextBox)grd_Travel_details.Rows[rowIndex].FindControl("txt_item_cost");


                    drCurrentRow = dtCurrentTable.NewRow();
                    drCurrentRow["RowNumber"] = i + 1;

                    dtCurrentTable.Rows[i - 1]["txt_item_description"] = txt_item_description.Text;
                    dtCurrentTable.Rows[i - 1]["txt_item_quantity"] = txt_item_quantity.Text;
                    dtCurrentTable.Rows[i - 1]["txt_item_cost"] = txt_item_cost.Text;

                    rowIndex++;
                }
                //dtCurrentTable.Rows.Add(drCurrentRow);
                //ViewState["CurrentTable"] = dtCurrentTable;

                //grd_Travel_details.DataSource = dtCurrentTable;
                //grd_Travel_details.DataBind();
                ViewState["CurrentTable"] = dtCurrentTable;
            }
        }
        else
        {
            Response.Write("ViewState is null");
        }
        //SetPreviousData();
    }
    #endregion

    protected void DG1_RowCreated(object sender, GridViewRowEventArgs e)
    {

    }
    protected void DG1_RowDataBound(object sender, GridViewRowEventArgs e)
    {

    }
    protected void DG2_RowCreated(object sender, GridViewRowEventArgs e)
    {

    }
    protected void DG2_RowDataBound(object sender, GridViewRowEventArgs e)
    {

    }
    #region NA
    //protected int available_balance_amount = 0;
    //protected decimal items_total_amount;
    //protected decimal items_total_amount_p;
    //private Random random = new Random();

    //public string Email_User_ID = System.Configuration.ConfigurationManager.AppSettings["EmailID"];
    //public string Email_Password = System.Configuration.ConfigurationManager.AppSettings["EmailPassword"];


    //SqlConnection con = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ToString());
    //SqlCommand logincommand1;
    //SqlDataReader objreader1;

    //protected void Page_Load(object sender, EventArgs e)
    //{
    //    if (!Page.IsPostBack)
    //    {


    //        //Disable Submit Button
    //        // .btn_submit.Attributes.Add("onclick", DisableTheButton(Me.Page, Me.btn_submit))

    //        string reference_no = "";

    //        try
    //        {
    //            reference_no = Request.QueryString["Session_Id"].ToString();
    //        }
    //        catch (Exception ex)
    //        {
    //            Response.Redirect("Invalid_Request.aspx");
    //        }

    //        if (Session["Form_A_Reference_No"].ToString() != reference_no)
    //            Response.Redirect("Invalid_Request.aspx");


    //        SetInitialRow();

    //        string financial_year = Session["Form_A_Entry_Financial_Year"].ToString();
    //        string department_division_id = Session["Department_Divison_Id"].ToString();
    //        //' Dim budget_main_head As String = Session["Form_A_Entry_Main_Head")
    //        string budget_head = Session["Form_A_Entry_Head"].ToString();
    //        string budget_sub_head = Session["Form_A_Entry_Sub_Head"].ToString();

    //        string budget_main_head_for_balance = Session["Form_A_Entry_Main_Head"].ToString();
    //        string budget_head_for_balance = Session["Form_A_Entry_Head"].ToString();
    //        string budget_sub_head_for_balance = Session["Form_A_Entry_Sub_Head"].ToString();

    //        // string budget_main_head_for_balance22 = Session["Form_A_Entry_Main_Head"];


    //        lbl_current_date.Text = DateTime.Now.ToString("dd/MMM/yyyy");


    //        // 'Get Department Name from the database
    //        SqlDataReader objreader1W;
    //        SqlCommand logincommand1W = new SqlCommand();
    //        logincommand1W.CommandText = "SELECT Department_Name FROM Department_Division_Master WHERE Department_Id = '" + department_division_id.ToString() + "'";
    //        logincommand1W.Connection = con;
    //        con.Open();
    //        objreader1W = logincommand1W.ExecuteReader();
    //        if (objreader1W.Read())
    //        {
    //            lbl_department_name.Text = objreader1W["Department_Name"].ToString();
    //        }
    //        con.Close();

    //        // 'Get Budget Head Description
    //        string budget_head_description = "";
    //        SqlDataReader objreader1PW;
    //        var logincommand1P = new SqlCommand();
    //        logincommand1P.CommandText = "SELECT Department_Name FROM Department_Division_Master  WHERE Department_Id = '" + department_division_id.ToString() + "'";
    //        logincommand1P.Connection = con;
    //        con.Open();
    //        objreader1PW = logincommand1P.ExecuteReader();
    //        if (objreader1PW.Read())
    //        {
    //            lbl_department_name.Text = objreader1PW["Department_Name"].ToString();
    //        }

    //        con.Close();

    //        //'if CInt(budget_main_head) < 10 Then
    //        //'    budget_main_head = "0" + "" + budget_main_head
    //        //'}

    //        //'if CInt(budget_head) < 10 Then
    //        //'    budget_head = "0" + "" + budget_head
    //        //'}

    //        //'if CInt(budget_sub_head) < 10 Then
    //        //'    budget_sub_head = "0" + "" + budget_sub_head
    //        //'}

    //        //' lbl_budget_head_details.Text = budget_main_head.ToString() +"/" + budget_head.ToString +"/" + budget_sub_head.ToString +"/" + budget_head_description.ToString

    //        lbl_budget_head_details.Text = budget_head.ToString() + "/" + budget_sub_head.ToString();
    //        lbl_Project_head_Title.Text = budget_head_description.ToString();

    //        //'-----------https://codepen.io/robcopeland/pen/rjXZra?__cf_chl_jschl_tk__=af32e380d626086c690ac7340f03e47c798f7f9d-1599682543-0-AZ2X4ypxiXMzho9p5nRt6GUKdD7KAF5jJlta_sdv7SFFzH_D0gu9EqjZoxPjl9I0UQ2D2j2r801hDIBSGSjTuExviK1DhHtRI_lmaJ_LycBNmYJwurFT-e05CVRV5bn3VTJUSUs02V3URR6m7sILHw-u2H8kn_eSQZgD48DZoQa5skErwSCdR6T3WVqvk-gCyXKSQfZGOvP1lGwaf4X3fzf8N6Tt2ahBuDUBFT_t83gzFPTAi_ZvtjBAxE0VKgM4xC5xIukBlSZoSsrAb72hb2Xt_PyAS7PX2FV-95jI2i-GS7EFQQahjJh5a6HAGKb294R88MIDN9zQfByuSJAYthpnY5AAt-mtol0lnF-KYPAR

    //        if (!Page.IsPostBack)
    //        {

    //            // 'Get Justification Record Count
    //            //Dim command5 As SqlCommand            
    //            SqlCommand command5;
    //            int approval_justification_record_count;
    //            //' Dim str5 As String = "SELECT COUNT(*) FROM Form_A_Justification_Details_Draft WHERE Reference_No = '" + reference_no.ToString() +"' "
    //            string str5 = "SELECT COUNT(*) FROM Form_A_Equipment_Justification_Details_Draft WHERE Reference_No = '" + reference_no.ToString() + "' ";
    //            command5 = new SqlCommand(str5, con);
    //            con.Open();
    //            approval_justification_record_count = command5.ExecuteNonQuery();
    //            con.Close();

    //            // 'Get Justification details from the database
    //            if (approval_justification_record_count > 0)
    //            {

    //                SqlDataReader objreader6;
    //                SqlCommand logincommand6 = new SqlCommand();
    //                //'logincommand6.CommandText = "SELECT Justification_Details FROM Form_A_Justification_Details_Draft WHERE Reference_No = '" + reference_no.ToString() +"' "
    //                logincommand6.CommandText = "SELECT Justification_Details FROM Form_A_Equipment_Justification_Details_Draft WHERE Reference_No = '" + reference_no.ToString() + "' ";
    //                logincommand6.Connection = con;
    //                con.Open();
    //                objreader6 = logincommand6.ExecuteReader();
    //                if (objreader6.Read())
    //                {
    //                    txt_justification.Text = objreader6["Justification_Details"].ToString();
    //                }
    //                con.Close();
    //            }


    //            // 'Get Items Record Count
    //            SqlCommand command7 = new SqlCommand();
    //            int approval_items_record_count;
    //            //' Dim str7 As String = "SELECT COUNT(*) FROM Form_A_Item_Details_Draft WHERE Reference_No = '" + reference_no.ToString() +"' "
    //            string str7 = "SELECT COUNT(*) FROM Form_A_Equipment_Item_Details_Draft WHERE Reference_No = '" + reference_no.ToString() + "' ";
    //            command7 = new SqlCommand(str7, con);
    //            con.Open();
    //            approval_items_record_count = command7.ExecuteNonQuery();
    //            con.Close();

    //            if (approval_items_record_count > 0)
    //            {
    //                //   'Bind Data to Gridview
    //                Bind_Item_Details();
    //                div_item_details.Visible = true;
    //            }

    //        }


    //        //'----------------Get Avalaible Balance-----
    //        SqlCommand command8 = new SqlCommand();
    //        int institute_approval_record_count;
    //        //'Dim str8 As String = "SELECT COUNT(*) FROM Institute_Approval_Master WHERE Financial_Year = '" + financial_year.ToString() +"' AND Department_Id = '" + department_division_id.ToString() +"' AND Main_Head = '" + budget_main_head_for_balance.ToString() +"' AND Head = '" + budget_head_for_balance.ToString() +"' AND Sub_Head = '" + budget_sub_head_for_balance.ToString() +"' AND Approval_Status IN ('Approved') "
    //        string str8 = "SELECT COUNT(*) FROM tbl_Individual_Consumables_form WHERE Financial_Year = '" + financial_year.ToString() + "' AND Department_Id = '" + department_division_id.ToString() + "' AND   Head = '" + budget_head_for_balance.ToString() + "' AND Sub_Head = '" + budget_sub_head_for_balance.ToString() + "' AND Approval_Status IN ('Approved') ";
    //        command8 = new SqlCommand(str8, con);
    //        con.Open();
    //        institute_approval_record_count = command8.ExecuteNonQuery();
    //        con.Close();



    //        int approved_amount = 0;
    //        int spent_amount = 0;
    //        int balance_amount = 0;

    //        if (institute_approval_record_count == 0)
    //        {

    //            // 'if No Approval is Submitted then Get the Approved Budget Head Amount from the database
    //            SqlDataReader objreader9;
    //            SqlCommand logincommand9 = new SqlCommand();
    //            // 'logincommand9.CommandText = "SELECT Approved_Amount FROM Budget_Master WHERE Financial_Year = '" + financial_year.ToString() +"' AND Department_Id = '" + department_division_id.ToString() +"' AND Main_Head = '" + budget_main_head_for_balance.ToString() +"' AND Head = '" + budget_head_for_balance.ToString() +"' AND Sub_Head = '" + budget_sub_head_for_balance.ToString() +"' "
    //            logincommand9.CommandText = "SELECT Approved_Amount FROM Budget_Master_Individual WHERE Financial_Year = '" + financial_year.ToString() + "' AND Department_Id = '" + department_division_id.ToString() + "' AND   Head = '" + budget_head_for_balance.ToString() + "' AND Sub_Head = '" + budget_sub_head_for_balance.ToString() + "' ";
    //            logincommand9.Connection = con;
    //            con.Open();
    //            objreader9 = logincommand9.ExecuteReader();
    //            if (objreader9.Read())
    //            {
    //                balance_amount = Convert.ToInt32(objreader9["Approved_Amount"].ToString());
    //                available_balance_amount = balance_amount;


    //                //  'lbl_budget_head_balance_amount.Text = String.Format("{0:C}", balance_amount)
    //                lbl_budget_head_balance_amount.Text = "Rs." + " " + balance_amount + ".00" + "";
    //            }
    //            con.Close();
    //        }
    //        else if (institute_approval_record_count > 0)
    //        {
    //            int estimated_amount = 0;
    //            int bill_amount = 0;

    //            SqlDataReader objreader9;
    //            SqlCommand logincommand9 = new SqlCommand();
    //            // 'logincommand9.CommandText = "SELECT Approved_Amount FROM Budget_Master WHERE Financial_Year = '" + financial_year.ToString() +"' AND Department_Id = '" + department_division_id.ToString() +"' AND Main_Head = '" + budget_main_head_for_balance.ToString() +"' AND Head = '" + budget_head_for_balance.ToString() +"' AND Sub_Head = '" + budget_sub_head_for_balance.ToString() +"' "
    //            logincommand9.CommandText = "SELECT Approved_Amount FROM Budget_Master_Individual WHERE Financial_Year = '" + financial_year.ToString() + "' AND Department_Id = '" + department_division_id.ToString() + "' AND Head = '" + budget_head_for_balance.ToString() + "' AND Sub_Head = '" + budget_sub_head_for_balance.ToString() + "' ";
    //            logincommand9.Connection = con;
    //            con.Open();
    //            objreader9 = logincommand9.ExecuteReader();
    //            if (objreader9.Read())
    //            {
    //                approved_amount = Convert.ToInt32(objreader9["Approved_Amount"].ToString());
    //            }
    //            con.Close();


    //            // 'Estimated Amount
    //            SqlDataReader objreader10;
    //            SqlCommand logincommand10 = new SqlCommand();
    //            //' logincommand10.CommandText = "SELECT CASE WHEN SUM(cast(Estimated_Amount as decimal(10,2))) is null then '0' ELSE SUM(cast(Estimated_Amount as decimal(10,2))) END as as Estimated_Amount FROM Institute_Approval_Master WHERE Financial_Year = '" + financial_year.ToString() +"' AND Department_Id = '" + department_division_id.ToString() +"' AND Main_Head = '" + budget_main_head_for_balance.ToString() +"' AND Head = '" + budget_head_for_balance.ToString() +"' AND Sub_Head = '" + budget_sub_head_for_balance.ToString() +"' AND Dean_Approval_Status IN ('Approved') AND Bill_Amount_Submit_Status = 'Pending'"
    //            logincommand10.CommandText = "SELECT CASE WHEN SUM(cast(Estimated_Amount as decimal(10,2))) is null then '0' ELSE SUM(cast(Estimated_Amount as decimal(10,2))) END as as Estimated_Amount FROM tbl_Individual_Consumables_form WHERE Financial_Year = '" + financial_year.ToString() + "' AND Department_Id = '" + department_division_id.ToString() + "' AND  Head = '" + budget_head_for_balance.ToString() + "' AND Sub_Head = '" + budget_sub_head_for_balance.ToString() + "' AND Dean_Approval_Status IN ('Approved') AND Bill_Amount_Submit_Status = 'Pending'";
    //            logincommand10.Connection = con;
    //            con.Open();
    //            objreader10 = logincommand10.ExecuteReader();
    //            if (objreader10.Read())
    //            {
    //                estimated_amount = Convert.ToInt32(objreader10["Estimated_Amount"].ToString());
    //            }
    //            con.Close();


    //            // 'Get Bill Amount/Actual Amount
    //            SqlDataReader objreader1P;
    //            SqlCommand logincommand1P9 = new SqlCommand();
    //            //  ' logincommand11.CommandText = "SELECT CASE WHEN SUM(cast(Bill_Amount as decimal(10,2))) is null then '0' ELSE SUM(cast(Bill_Amount as decimal(10,2))) END as Billed_Amount FROM Institute_Approval_Master WHERE Financial_Year = '" + financial_year.ToString() +"' AND Department_Id = '" + department_division_id.ToString() +"' AND Main_Head = '" + budget_main_head_for_balance.ToString() +"' AND Head = '" + budget_head_for_balance.ToString() +"' AND Sub_Head = '" + budget_sub_head_for_balance.ToString() +"' AND Approval_Status IN ('Approved') AND Bill_Amount_Submit_Status = 'Processed' "
    //            logincommand1P9.CommandText = "SELECT CASE WHEN SUM(cast(Bill_Amount as decimal(10,2))) is null then '0' ELSE SUM(cast(Bill_Amount as decimal(10,2))) END as Billed_Amount FROM tbl_Individual_Consumables_form WHERE Financial_Year = '" + financial_year.ToString() + "' AND Department_Id = '" + department_division_id.ToString() + "' AND   Head = '" + budget_head_for_balance.ToString() + "' AND Sub_Head = '" + budget_sub_head_for_balance.ToString() + "' AND Approval_Status IN ('Approved') AND Bill_Amount_Submit_Status = 'Processed' ";
    //            logincommand1P9.Connection = con;
    //            con.Open();
    //            objreader1P = logincommand1P9.ExecuteReader();
    //            if (objreader1P.Read())
    //            {
    //                bill_amount = Convert.ToInt32(objreader1P["Billed_Amount"].ToString());
    //            }
    //            con.Close();

    //            balance_amount = approved_amount - (estimated_amount + bill_amount);
    //            available_balance_amount = balance_amount;
    //            lbl_budget_head_balance_amount.Text = "Rs." + " " + balance_amount + ".00" + "";

    //        }


    //        //'Get Head Name from the database
    //        string faculty_name = "";
    //        string Faculty_Salutation = "";

    //        SqlDataReader objreader12;
    //        SqlCommand logincommand12 = new SqlCommand();
    //        //'logincommand12.CommandText = "SELECT Department_Head_Incharge_Name,Department_Head_Incharge_Salutation FROM Department_Division_Master WHERE Department_Id = '" + department_division_id.ToString() +"'"

    //        logincommand12.CommandText = "SELECT  Faculty_Salutation, Faculty_Name FROM Faculty_Master WHERE Department_Id = '" + department_division_id.ToString() + "'";
    //        logincommand12.Connection = con;
    //        con.Open();
    //        objreader12 = logincommand12.ExecuteReader();
    //        if (objreader12.Read())
    //        {
    //            Faculty_Salutation = objreader12["Faculty_Salutation"].ToString();
    //            faculty_name = objreader12["Faculty_Name"].ToString();

    //            Session["Head_FullName"] = faculty_name.ToString();

    //            lbl_user_name.Text = faculty_name.ToString() + ", " + Faculty_Salutation.ToString();
    //        }
    //        con.Close();


    //        //'Get Intitute Approval Creation Permission Details from the database

    //        string institute_approval_creation_permission_status = "";
    //        SqlDataReader objreader13;
    //        SqlCommand logincommand13 = new SqlCommand();
    //        logincommand13.CommandText = "SELECT Institute_Approval_Permission_Status FROM Institute_Approval_Permission_Details";
    //        logincommand13.Connection = con;
    //        con.Open();
    //        objreader13 = logincommand13.ExecuteReader();
    //        if (objreader13.Read())
    //        {
    //            institute_approval_creation_permission_status = objreader13["Institute_Approval_Permission_Status"].ToString();

    //            if (institute_approval_creation_permission_status == "No")
    //            {
    //                Response.Redirect("Institute_Approval_Creation_Permission_Denied.aspx");
    //            }
    //        }
    //        con.Close();


    //        if (!Page.IsPostBack)
    //        {
    //            FillCapctha();
    //        }

    //    }
    //}


    //private string DisableTheButton(Control pge, Control btn)
    //{
    //    var sb = new StringBuilder();
    //    sb.Append("if (typeof(Page_ClientValidate) == 'function') {");
    //    sb.Append("if (Page_ClientValidate() == false) { return false; }} ");
    //    sb.Append("if (confirm('Are you sure to proceed?') == false) { return false; } ");
    //    sb.Append("this.value = 'Please wait ...';");
    //    sb.Append("this.disabled = true;");
    //    sb.Append(pge.Page.GetPostBackEventReference(btn));
    //    sb.Append(";");
    //    return sb.ToString();
    //}

    //private string GenerateRandomCode()
    //{
    //    string s = "";
    //    for (int i = 0; i <= 5; i++)
    //        s = string.Concat(s, this.random.Next(10).ToString());
    //    return s;
    //}




    //public void message(string temp)
    //{
    //    string strscript;
    //    strscript = "<script>";
    //    strscript = strscript + "alert('" + temp + "');";
    //    strscript = strscript + "</script>";
    //    Page.RegisterStartupScript("ClientScript", strscript.ToString());
    //}


    //private void Bind_Item_Details()
    //{
    //    string reference_no;
    //    reference_no = Request.QueryString["Session_Id"].ToString();
    //    string constr = System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ToString();
    //    using (var con = new SqlConnection(constr))
    //    {
    //        // Dim str1 As String = "SELECT * FROM Form_A_Item_Details_Draft WHERE Reference_No = '" & reference_no.ToString() & "' "
    //        string str1 = "SELECT * FROM Form_A_Equipment_Item_Details_Draft WHERE Reference_No = '" + reference_no.ToString() + "' ";
    //        using (var cmd = new SqlCommand(str1))
    //        {
    //            using (var sda = new SqlDataAdapter())
    //            {
    //                cmd.Connection = con;
    //                sda.SelectCommand = cmd;
    //                using (var dt = new DataTable())
    //                {
    //                    sda.Fill(dt);
    //                    DG1.DataSource = dt;
    //                    DG1.DataBind();
    //                }
    //            }
    //        }
    //    }
    //}


    //private void Bind_Item_Details_For_Preview()
    //{
    //    string reference_no;
    //    reference_no = Request.QueryString["Session_Id"].ToString();
    //    string constr = System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ToString();
    //    using (var con = new SqlConnection(constr))
    //    {
    //        // Dim str1 As String = "SELECT * FROM Form_A_Item_Details_Draft WHERE Reference_No = '" & reference_no.ToString() & "' "
    //        string str1 = "SELECT * FROM Form_A_Equipment_Item_Details_Draft WHERE Reference_No = '" + reference_no.ToString() + "' ";
    //        using (var cmd1 = new SqlCommand(str1))
    //        {
    //            using (var sda1 = new SqlDataAdapter())
    //            {
    //                cmd1.Connection = con;
    //                sda1.SelectCommand = cmd1;
    //                using (var dt1 = new DataTable())
    //                {
    //                    sda1.Fill(dt1);
    //                    DG2.DataSource = dt1;
    //                    DG2.DataBind();
    //                }
    //            }
    //        }
    //    }
    //}


    //protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
    //{
    //    var con = new System.Data.SqlClient.SqlConnection();
    //    con.ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ToString();
    //    if (e.Row.RowType == DataControlRowType.DataRow)
    //    {
    //        Label lbl_b_amount = (Label)e.Row.FindControl("lbl_items_total_amount");
    //        items_total_amount += Convert.ToDecimal(DataBinder.Eval(e.Row.DataItem, "Item_Cost"));
    //    }

    //    if (e.Row.RowType == DataControlRowType.Footer)
    //    {
    //        Label lbl_b_amount = (Label)e.Row.FindControl("lbl_items_total_amount");
    //        lbl_b_amount.Text = "Rs." + " " + items_total_amount.ToString() + ".00" + "";
    //    }

    //    foreach (GridViewRow row in DG1.Rows)
    //    {
    //        string record_id = DG1.DataKeys[row.RowIndex].Values[0].ToString();
    //        string reference_no = DG1.DataKeys[row.RowIndex].Values[1].ToString();
    //        Label lbl_item_amount = (Label)row.FindControl("lbl_item_amount");
    //        SqlDataReader objreader1;
    //        var logincommand1 = new SqlCommand();
    //        // logincommand1.CommandText = "SELECT Item_Cost FROM Form_A_Item_Details_Draft WHERE Record_Id = '" & record_id.ToString() & "'  AND Reference_No = '" & reference_no.ToString() & "' "
    //        logincommand1.CommandText = "SELECT Item_Cost FROM Form_A_Equipment_Item_Details_Draft WHERE Record_Id = '" + record_id.ToString() + "'  AND Reference_No = '" + reference_no.ToString() + "' ";
    //        logincommand1.Connection = con;
    //        con.Open();
    //        objreader1 = logincommand1.ExecuteReader();
    //        if (objreader1.Read())
    //        {
    //            lbl_item_amount.Text = "Rs." + " " + objreader1["Item_Cost"].ToString() + ".00" + "";
    //        }

    //        con.Close();
    //    }
    //}

    //protected void GrdView1_RowCreated(object sender, GridViewRowEventArgs e)
    //{
    //    if (e.Row.RowType == DataControlRowType.Header)
    //    {
    //    }
    //}


    //protected void GridView2_RowDataBound(object sender, GridViewRowEventArgs e)
    //{
    //    var con = new System.Data.SqlClient.SqlConnection();
    //    con.ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ToString();
    //    if (e.Row.RowType == DataControlRowType.DataRow)
    //    {
    //        items_total_amount_p += Convert.ToDecimal(DataBinder.Eval(e.Row.DataItem, "Item_Cost"));
    //    }

    //    if (e.Row.RowType == DataControlRowType.Footer)
    //    {
    //        Label lbl_items_total_amount_p = (Label)e.Row.FindControl("lbl_items_total_amount_p");
    //        lbl_items_total_amount_p.Text = "Rs." + " " + items_total_amount_p.ToString() + ".00" + "";
    //    }

    //    foreach (GridViewRow row in DG2.Rows)
    //    {
    //        string record_id = DG2.DataKeys[row.RowIndex].Values[0].ToString();
    //        string reference_no = DG2.DataKeys[row.RowIndex].Values[1].ToString();
    //        Label lbl_item_amount_p = (Label)row.FindControl("lbl_item_amount_p");
    //        SqlDataReader objreader1;
    //        var logincommand1 = new SqlCommand();
    //        logincommand1.CommandText = "SELECT Item_Cost FROM Form_A_Item_Details_Draft WHERE Record_Id = '" + record_id.ToString() + "'  AND Reference_No = '" + reference_no.ToString() + "' ";
    //        logincommand1.Connection = con;
    //        con.Open();
    //        objreader1 = logincommand1.ExecuteReader();
    //        if (objreader1.Read())
    //        {
    //            lbl_item_amount_p.Text = "Rs." + " " + objreader1["Item_Cost"].ToString() + ".00" + "";
    //        }

    //        con.Close();
    //    }
    //}

    //protected void GrdView2_RowCreated(object sender, GridViewRowEventArgs e)
    //{
    //    if (e.Row.RowType == DataControlRowType.Header)
    //    {
    //    }
    //}


    //private void FillCapctha()
    //{
    //    try
    //    {
    //        var random = new Random();
    //        // Dim combination As String = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz"
    //        string combination = "123456789";
    //        var captcha = new StringBuilder();
    //        for (int i = 0; i <= 3; i++)
    //            captcha.Append(combination[random.Next(combination.Length)]);
    //        Session["captcha"] = captcha.ToString();
    //        imgCaptcha.ImageUrl = "Captcha/Captcha.aspx?" + DateTime.Now.Ticks.ToString();
    //    }
    //    catch
    //    {
    //        throw;
    //    }
    //}


    //protected void btn_add_items_Click(object sender, EventArgs e)
    //{
    //    var con = new System.Data.SqlClient.SqlConnection();
    //    con.ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ToString();
    //    long item_cost_amount = Convert.ToInt32(txt_item_cost.Text);
    //    string reference_no = "";
    //    try
    //    {
    //        reference_no = Request.QueryString["Session_Id"].ToString();
    //    }
    //    catch (Exception ex)
    //    {
    //        Response.Redirect("Invalid_Request.aspx");
    //    }

    //    if (Session["Form_A_Reference_No"].ToString() != reference_no)
    //    {
    //        Response.Redirect("Invalid_Request.aspx");
    //    }

    //    if (txt_item_description.Text == "")
    //    {
    //        message("Please Enter Item Description");
    //        txt_item_description.Focus();
    //    }
    //    else if (txt_item_quantity.Text == "")
    //    {
    //        message("Please Enter Item Quantity");
    //        txt_item_quantity.Focus();
    //    }
    //    else if (txt_item_quantity.Text == "0")
    //    {
    //        message("Quantity Should Be Greater Than Zero");
    //        txt_item_quantity.Text = "";
    //        txt_item_quantity.Focus();
    //    }
    //    else if (txt_item_quantity.Text == "00")
    //    {
    //        message("Quantity Should Be Greater Than Zero");
    //        txt_item_quantity.Text = "";
    //        txt_item_quantity.Focus();
    //    }
    //    else if (txt_item_quantity.Text == "000")
    //    {
    //        message("Quantity Should Be Greater Than Zero");
    //        txt_item_quantity.Text = "";
    //        txt_item_quantity.Focus();
    //    }
    //    else if (txt_item_quantity.Text == "0000")
    //    {
    //        message("Quantity Should Be Greater Than Zero");
    //        txt_item_quantity.Text = "";
    //        txt_item_quantity.Focus();
    //    }
    //    else if (txt_item_quantity.Text == "00000")
    //    {
    //        message("Quantity Should Be Greater Than Zero");
    //        txt_item_quantity.Text = "";
    //        txt_item_quantity.Focus();
    //    }
    //    else if (txt_item_cost.Text == "")
    //    {
    //        message("Please Enter Item Cost");
    //        txt_item_cost.Focus();
    //    }
    //    else if (txt_item_cost.Text == "0")
    //    {
    //        message("Amount Should Be Greater Than Zero");
    //        txt_item_cost.Text = "";
    //        txt_item_cost.Focus();
    //    }
    //    else if (txt_item_cost.Text == "00")
    //    {
    //        message("Amount Should Be Greater Than Zero");
    //        txt_item_cost.Text = "";
    //        txt_item_cost.Focus();
    //    }
    //    else if (txt_item_cost.Text == "000")
    //    {
    //        message("Amount Should Be Greater Than Zero");
    //        txt_item_cost.Text = "";
    //        txt_item_cost.Focus();
    //    }
    //    else if (txt_item_cost.Text == "0000")
    //    {
    //        message("Amount Should Be Greater Than Zero");
    //        txt_item_cost.Text = "";
    //        txt_item_cost.Focus();
    //    }
    //    else if (txt_item_cost.Text == "00000")
    //    {
    //        message("Amount Should Be Greater Than Zero");
    //        txt_item_cost.Text = "";
    //        txt_item_cost.Focus();
    //    }
    //    else if (txt_item_cost.Text == "000000")
    //    {
    //        message("Amount Should Be Greater Than Zero");
    //        txt_item_cost.Text = "";
    //        txt_item_cost.Focus();
    //    }
    //    else if (txt_item_cost.Text == "0000000")
    //    {
    //        message("Amount Should Be Greater Than Zero");
    //        txt_item_cost.Text = "";
    //        txt_item_cost.Focus();
    //    }
    //    else if (txt_item_cost.Text == "00000000")
    //    {
    //        message("Amount Should Be Greater Than Zero");
    //        txt_item_cost.Text = "";
    //        txt_item_cost.Focus();
    //    }
    //    else if (txt_item_cost.Text == "000000000")
    //    {
    //        message("Amount Should Be Greater Than Zero");
    //        txt_item_cost.Text = "";
    //        txt_item_cost.Focus();
    //    }
    //    else if (txt_item_cost.Text == "0000000000")
    //    {
    //        message("Amount Should Be Greater Than Zero");
    //        txt_item_cost.Text = "";
    //        txt_item_cost.Focus();
    //    }
    //    else
    //    {
    //        string financial_year = Session["Form_A_Entry_Financial_Year"].ToString();
    //        string department_division_id = Session["Department_Divison_Id"].ToString();
    //        // Dim budget_main_head As String = Session["Form_A_Entry_Main_Head")
    //        string budget_head = Session["Form_A_Entry_Head"].ToString();
    //        string budget_sub_head = Session["Form_A_Entry_Sub_Head"].ToString();
    //        string submitted_on = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

    //        // Delete Name of User from the database
    //        var cmd1 = new SqlCommand();
    //        cmd1.CommandType = System.Data.CommandType.Text;
    //        cmd1.CommandText = "DELETE FROM Form_A_Equipment_User_Name_Details_Draft WHERE Reference_No = '" + reference_no.ToString() + "' ";
    //        cmd1.Connection = con;
    //        con.Open();
    //        cmd1.ExecuteNonQuery();
    //        con.Close();


    //        // Insert Item details in the database
    //        string commandstr3;
    //        var logincommand3 = new SqlCommand();
    //        // commandstr3 = "INSERT INTO Form_A_Item_Details_Draft(Department_Id,Reference_No,Item_Description,Item_Quantity,Item_Cost,Submitted_On) values ( '" & department_division_id.ToString() & "','" & reference_no.ToString() & "','" & txt_item_description.Text.ToString() & "','" & txt_item_quantity.Text.ToString() & "','" & txt_item_cost.Text.ToString() & "','" & submitted_on.ToString() & "')"
    //        commandstr3 = "INSERT INTO Form_A_Equipment_Item_Details_Draft(Department_Id,Reference_No,Item_Description,Item_Quantity,Item_Cost,Submitted_On) values ( '" + department_division_id.ToString() + "','" + reference_no.ToString() + "','" + txt_item_description.Text.ToString() + "','" + txt_item_quantity.Text.ToString() + "','" + txt_item_cost.Text.ToString() + "','" + submitted_on.ToString() + "')";
    //        logincommand3.CommandText = commandstr3;
    //        logincommand3.Connection = con;
    //        con.Open();
    //        logincommand3.ExecuteNonQuery();
    //        con.Close();


    //        // Delete Justification details  from the database
    //        var cmd4 = new SqlCommand();
    //        cmd4.CommandType = System.Data.CommandType.Text;
    //        // cmd4.CommandText = "DELETE FROM Form_A_Justification_Details_Draft WHERE Reference_No = '" & reference_no.ToString() & "' "
    //        cmd4.CommandText = "DELETE FROM Form_A_Equipment_Justification_Details_Draft WHERE Reference_No = '" + reference_no.ToString() + "' ";
    //        cmd4.Connection = con;
    //        con.Open();
    //        cmd4.ExecuteNonQuery();
    //        con.Close();

    //        // Insert Justification details in the database
    //        string commandstr5;
    //        var logincommand5 = new SqlCommand();
    //        // commandstr5 = "INSERT INTO Form_A_Justification_Details_Draft(Department_Id,Reference_No,Justification_Details,Submitted_On) values ( '" & department_division_id.ToString() & "','" & reference_no.ToString() & "','" & txt_justification.Text.ToString() & "','" & submitted_on.ToString() & "')"
    //        commandstr5 = "INSERT INTO Form_A_Equipment_Justification_Details_Draft(Department_Id,Reference_No,Justification_Details,Submitted_On) values ( '" + department_division_id.ToString() + "','" + reference_no.ToString() + "','" + txt_justification.Text.ToString() + "','" + submitted_on.ToString() + "')";
    //        logincommand5.CommandText = commandstr5;
    //        logincommand5.Connection = con;
    //        con.Open();
    //        logincommand5.ExecuteNonQuery();
    //        con.Close();
    //        Response.Redirect(string.Format("Individual_Approval_PostBack_Processing_Equipment.aspx?Session_Id={0}", reference_no));
    //    }
    //}


    //protected void btn_preview_Click(object sender, EventArgs e)
    //{
    //    var con = new System.Data.SqlClient.SqlConnection();
    //    con.ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ToString();
    //    string financial_year = Session["Form_A_Entry_Financial_Year"].ToString();
    //    string department_division_id = Session["Department_Divison_Id"].ToString();
    //    string budget_main_head = Session["Form_A_Entry_Main_Head"].ToString();
    //    string budget_head = Session["Form_A_Entry_Head"].ToString();
    //    string budget_sub_head = Session["Form_A_Entry_Sub_Head"].ToString();
    //    string submitted_on = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    //    string reference_no = "";
    //    try
    //    {
    //        reference_no = Request.QueryString["Session_Id"].ToString();
    //    }
    //    catch (Exception ex)
    //    {
    //        Response.Redirect("Invalid_Request.aspx");
    //    }

    //    if (Session["Form_A_Reference_No"].ToString() != reference_no)
    //    {
    //        Response.Redirect("Invalid_Request.aspx");
    //    }

    //    // Get Items Count from the database
    //    SqlCommand command1;
    //    int items_record_count;
    //    string str1 = "SELECT COUNT(*) FROM Form_A_Item_Details_Draft WHERE Reference_No = '" + reference_no.ToString() + "' ";
    //    command1 = new SqlCommand(str1, con);
    //    con.Open();
    //    items_record_count = command1.ExecuteNonQuery();
    //    con.Close();

    //    // Get Form-A Justification Record Count from the database
    //    SqlCommand command2;
    //    int justification_record_count;
    //    string str2 = "SELECT COUNT(*) FROM Form_A_Justification_Details_Draft WHERE Reference_No = '" + reference_no.ToString() + "' ";
    //    command2 = new SqlCommand(str2, con);
    //    con.Open();
    //    justification_record_count = command2.ExecuteNonQuery();
    //    con.Close();
    //    if (items_record_count == 0)
    //    {
    //        message("Please Enter atleast one item for Form-A");
    //        txt_item_description.Focus();
    //    }
    //    else if (txt_justification.Text == "")
    //    {
    //        message("Please Enter Your Justification");
    //        txt_justification.Focus();
    //    }
    //    else
    //    {

    //        // If Item details are full entered by the user then only allow to insert in the database
    //        if (txt_item_description.Text != "" & txt_item_quantity.Text != "" & txt_item_cost.Text != "")
    //        {

    //            // Insert Item details in the database
    //            string commandstr3;
    //            var logincommand3 = new SqlCommand();
    //            commandstr3 = "INSERT INTO Form_A_Item_Details_Draft(Department_Id,Reference_No,Item_Description,Item_Quantity,Item_Cost,Submitted_On) values ( '" + department_division_id.ToString() + "','" + reference_no.ToString() + "','" + txt_item_description.Text.ToString() + "','" + txt_item_quantity.Text.ToString() + "','" + txt_item_cost.Text.ToString() + "','" + submitted_on.ToString() + "')";
    //            logincommand3.CommandText = commandstr3;
    //            logincommand3.Connection = con;
    //            con.Open();
    //            logincommand3.ExecuteNonQuery();
    //            con.Close();
    //        }

    //        lbl_current_date_p.Text = DateTime.Now.ToString("dd/MMM/yyyy");
    //        lbl_department_name_p.Text = lbl_department_name.Text;
    //        lbl_name_of_user.Text = lbl_user_name.Text.ToString();
    //        lbl_budget_head_details_p.Text = lbl_budget_head_details.Text;
    //        lbl_justification_p.Text = txt_justification.Text.ToString();
    //        Bind_Item_Details_For_Preview();
    //        Panel1.Visible = false;
    //        Panel2.Visible = true;
    //    }
    //}

    //protected void btn_edit_Click(object sender, EventArgs e)
    //{
    //    Panel1.Visible = true;
    //    Panel2.Visible = false;
    //}



    //protected void btn_submit_Click(object sender, EventArgs e)
    //{
    //    var con = new System.Data.SqlClient.SqlConnection();
    //    con.ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConStr"].ToString();

    //    // Get Client IP Address
    //    string ipaddress;
    //    ipaddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
    //    if (string.IsNullOrEmpty(ipaddress) | ipaddress == null)
    //    {
    //        ipaddress = Request.ServerVariables["REMOTE_ADDR"];
    //    }

    //    Session["User_IP_Address"] = ipaddress;
    //    string financial_year = Session["Form_A_Entry_Financial_Year"].ToString();
    //    string department_division_id = Session["Department_Divison_Id"].ToString();
    //    string budget_main_head = Session["Form_A_Entry_Main_Head"].ToString();
    //    string budget_head = Session["Form_A_Entry_Head"].ToString();
    //    string budget_sub_head = Session["Form_A_Entry_Sub_Head"].ToString();
    //    string reference_no = Request.QueryString["Session_Id"].ToString();

    //    // Try
    //    // reference_no = Request.QueryString("Session_Id").ToString()
    //    // Catch ex As Exception
    //    // Response.Redirect("Invalid_Request.aspx")
    //    // End Try

    //    // If Session["Form_A_Reference_No") <> reference_no Then
    //    // Response.Redirect("Invalid_Request.aspx")
    //    // End If

    //    // Get Items Count from the database
    //    SqlCommand command1;
    //    int items_record_count;
    //    string str1 = "SELECT COUNT(*) FROM Form_A_Item_Details_Draft WHERE Reference_No = '" + reference_no.ToString() + "' ";
    //    command1 = new SqlCommand(str1, con);
    //    con.Open();
    //    items_record_count = command1.ExecuteNonQuery();
    //    con.Close();

    //    // Get Form-A Justification Record Count from the database
    //    SqlCommand command2;
    //    int justification_record_count;
    //    string str2 = "SELECT COUNT(*) FROM Form_A_Justification_Details_Draft WHERE Reference_No = '" + reference_no.ToString() + "' ";
    //    command2 = new SqlCommand(str2, con);
    //    con.Open();
    //    justification_record_count = command2.ExecuteNonQuery();
    //    con.Close();
    //    if (items_record_count == 0)
    //    {
    //        message("Please Enter atleast one item for Form-A");
    //       // txt_item_description.Focus();
    //    }
    //    else if (justification_record_count == 0)
    //    {
    //        message("Please Enter Your Justification");
    //        txt_justification.Focus();
    //    }
    //    else if (txt_captcha_code.Text == "")
    //    {
    //        message("Please Enter Security Code");
    //        txt_captcha_code.Focus();
    //    }
    //    else if (txt_captcha_code.Text.Length < 4)
    //    {
    //        message("Please Enter Four Digit Security Code");
    //    }
    //    else if (Session["captcha"].ToString() != txt_captcha_code.Text)
    //    {
    //        message("You Have Entered Wrong Security Code");
    //        txt_captcha_code.Text = "";
    //        txt_captcha_code.Focus();
    //    }
    //    else
    //    {
    //        string submitted_on = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

    //        // Get Added Items Total Amount from the database
    //        var form_A_total_amount = default(long);
    //        SqlDataReader objreader6;
    //        var logincommand6 = new SqlCommand();
    //        logincommand6.CommandText = "SELECT SUM(Item_Cost) as Form_A_Items_Total_Amount FROM Form_A_Item_Details_Draft WHERE Reference_No = '" + reference_no.ToString() + "' AND Department_Id = '" + department_division_id.ToString() + "' ";
    //        logincommand6.Connection = con;
    //        con.Open();
    //        objreader6 = logincommand6.ExecuteReader();
    //        if (objreader6.Read())
    //        {
    //            form_A_total_amount = Convert.ToInt32(objreader6["Form_A_Items_Total_Amount"].ToString());
    //        }

    //        con.Close();
    //        if (form_A_total_amount > available_balance_amount)
    //        {
    //            message("Items total cost is more than available balance");
    //            lbl_error_message.Visible = true;
    //            string user_message = "Approval total cost is more than available balance. Current available balance is Rs.";
    //            lbl_error_message.Text = user_message + " " + available_balance_amount + ".00" + "";
    //        }
    //        else if (form_A_total_amount <= available_balance_amount)
    //        {
    //            lbl_error_message.Visible = false;

    //            // Insert Form-A Justification Details in the database
    //            string commandstr7;
    //            var logincommand7 = new SqlCommand();
    //            commandstr7 = "INSERT INTO Form_A_Justification_Details(Department_Id,Reference_No,Justification_Details,Submitted_On) values ( '" + department_division_id.ToString() + "','" + reference_no.ToString() + "','" + txt_justification.Text.ToString() + "','" + submitted_on.ToString() + "')";
    //            logincommand7.CommandText = commandstr7;
    //            logincommand7.Connection = con;
    //            con.Open();
    //            logincommand7.ExecuteNonQuery();
    //            con.Close();

    //            // Insert Form-A Item Details in the database
    //            string item_description = "";
    //            string item_quantity = "";
    //            string item_cost = "";
    //            SqlDataReader objreader8;
    //            var logincommand8 = new SqlCommand();
    //            logincommand8.CommandText = "SELECT * FROM Form_A_Item_Details_Draft WHERE Reference_No = '" + reference_no.ToString() + "' ";
    //            logincommand8.Connection = con;
    //            con.Open();
    //            objreader8 = logincommand8.ExecuteReader();
    //            while (objreader8.Read())
    //            {
    //                item_description = objreader8["Item_Description"].ToString();
    //                item_quantity = objreader8["Item_Quantity"].ToString();
    //                item_cost = objreader8["Item_Cost"].ToString();

    //                // ------Insert Item Details in the database----
    //                string commandstr9;
    //                var logincommand9 = new SqlCommand();
    //                commandstr9 = "INSERT INTO Form_A_Item_Details(Department_Id,Reference_No,Item_Description,Item_Quantity,Item_Cost,Submitted_On) values ( '" + department_division_id.ToString() + "','" + reference_no.ToString() + "','" + item_description.ToString() + "','" + item_quantity.ToString() + "','" + item_cost.ToString() + "','" + submitted_on.ToString() + "')";
    //                logincommand9.CommandText = commandstr9;
    //                logincommand9.Connection = con;
    //                logincommand9.ExecuteNonQuery();
    //            }

    //            con.Close();
    //            string form_A_submitted_on = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

    //            // -------------Generate Approval Number---------

    //            // ---Get Approval Record Count----
    //            int approval_sequence_no = 0;
    //            SqlCommand command11;
    //            int approval_record_count;
    //            string str11 = "SELECT COUNT(*) FROM Approval_Numbers WHERE Financial_Year = '" + financial_year.ToString() + "' ";
    //            command11 = new SqlCommand(str11, con);
    //            con.Open();
    //            approval_record_count = command11.ExecuteNonQuery();
    //            con.Close();
    //            if (approval_record_count > 0)
    //            {
    //                SqlDataReader objreader12;
    //                var logincommand12 = new SqlCommand();
    //                logincommand12.CommandText = "SELECT MAX(Approval_Sequence_No) AS Last_Sequence_No FROM Approval_Numbers WHERE Financial_Year = '" + financial_year.ToString() + "' ";
    //                logincommand12.Connection = con;
    //                con.Open();
    //                objreader12 = logincommand12.ExecuteReader();
    //                if (objreader12.Read())
    //                {
    //                    approval_sequence_no = Convert.ToInt32(objreader12["Last_Sequence_No"].ToString());
    //                    approval_sequence_no = approval_sequence_no + 1;
    //                }

    //                con.Close();
    //            }
    //            else if (approval_record_count == 0)
    //            {
    //                approval_sequence_no = 0;
    //            }

    //            // ----------Approval Number----------
    //            string approval_number = "BITS/INST/" + "" + approval_sequence_no.ToString().ToString();
    //            string approval_no_generated_on = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");


    //            // Insert Approval Details in the database
    //            string approval_type = "Institute Approval";
    //            string commandstr13;
    //            var logincommand13 = new SqlCommand();
    //            commandstr13 = "INSERT INTO Approval_Numbers(Financial_Year,Approval_Type,Approval_No,Approval_Sequence_No,Created_On) values ('" + financial_year.ToString() + "','" + approval_type.ToString() + "' ,'" + approval_number.ToString() + "','" + approval_sequence_no + "','" + approval_no_generated_on.ToString() + "')";
    //            logincommand13.CommandText = commandstr13;
    //            logincommand13.Connection = con;
    //            con.Open();
    //            logincommand13.ExecuteNonQuery();
    //            con.Close();



    //            // ---For Dean Admin Only---
    //            if (form_A_total_amount <= 100000L)
    //            {
    //                string commandstr14;
    //                var logincommand14 = new SqlCommand();
    //                commandstr14 = "INSERT INTO Institute_Approval_Master(Approval_Id,Approval_No,Financial_Year,Department_Id,Main_Head,Head,Sub_Head,Approval_User,Estimated_Amount,Estimated_Billing_Amount,Amount_Approved_By_Dean_Director,Bill_Amount,Bill_Amount_Submit_Status,Approval_Status,Submitted_On,Submitted_By,Submit_IP_Address,Dean_Approval_Status,Director_Approval_Required,Director_Approval_Status) values ('" + reference_no.ToString() + "','" + approval_number.ToString() + "','" + financial_year.ToString() + "','" + department_division_id.ToString() + "' ,'" + budget_main_head.ToString() + "','" + budget_head.ToString() + "','" + budget_sub_head.ToString() + "','" + lbl_user_name.Text.ToString() + "','" + form_A_total_amount + "','0','0','0','Pending','Pending','" + form_A_submitted_on.ToString() + "','" + Session["Head_FullName"].ToString() + "','" + ipaddress.ToString() + "','Pending','No','Approval Not Required')";
    //                logincommand14.CommandText = commandstr14;
    //                logincommand14.Connection = con;
    //                con.Open();
    //                logincommand14.ExecuteNonQuery();
    //                con.Close();
    //            }

    //            // ---For Dean Admin and Director---
    //            else if (form_A_total_amount > 100000L)
    //            {
    //                string commandstr14;
    //                var logincommand14 = new SqlCommand();
    //                commandstr14 = "INSERT INTO Institute_Approval_Master(Approval_Id,Approval_No,Financial_Year,Department_Id,Main_Head,Head,Sub_Head,Approval_User,Estimated_Amount,Estimated_Billing_Amount,Amount_Approved_By_Dean_Director,Bill_Amount,Bill_Amount_Submit_Status,Approval_Status,Submitted_On,Submitted_By,Submit_IP_Address,Dean_Approval_Status,Director_Approval_Required,Director_Approval_Status) values ('" + reference_no.ToString() + "','" + approval_number.ToString() + "','" + financial_year.ToString() + "','" + department_division_id.ToString() + "' ,'" + budget_main_head.ToString() + "','" + budget_head.ToString() + "','" + budget_sub_head.ToString() + "','" + lbl_user_name.Text.ToString() + "','" + form_A_total_amount + "','0','0','0','Pending','Pending','" + form_A_submitted_on.ToString() + "','" + Session["Head_FullName"].ToString() + "','" + ipaddress.ToString() + "','Pending','Yes','Pending')";
    //                logincommand14.CommandText = commandstr14;
    //                logincommand14.Connection = con;
    //                con.Open();
    //                logincommand14.ExecuteNonQuery();
    //                con.Close();
    //            }

    //            // Delete From Draft Tables
    //            var cmd13 = new SqlCommand();
    //            cmd13.CommandType = System.Data.CommandType.Text;
    //            cmd13.CommandText = "DELETE FROM Form_A_User_Name_Details_Draft WHERE Reference_No = '" + reference_no.ToString() + "' ";
    //            cmd13.Connection = con;
    //            con.Open();
    //            cmd13.ExecuteNonQuery();
    //            con.Close();
    //            var cmd14 = new SqlCommand();
    //            cmd14.CommandType = System.Data.CommandType.Text;
    //            cmd14.CommandText = "DELETE FROM Form_A_Item_Details_Draft WHERE Reference_No = '" + reference_no.ToString() + "' ";
    //            cmd14.Connection = con;
    //            con.Open();
    //            cmd14.ExecuteNonQuery();
    //            con.Close();
    //            var cmd15 = new SqlCommand();
    //            cmd15.CommandType = System.Data.CommandType.Text;
    //            cmd15.CommandText = "DELETE FROM Form_A_Justification_Details_Draft WHERE Reference_No = '" + reference_no.ToString() + "' ";
    //            cmd15.Connection = con;
    //            con.Open();
    //            cmd15.ExecuteNonQuery();
    //            con.Close();

    //            // Get Approval User Details from the database
    //            string department_id = Session["Department_Divison_Id"].ToString();
    //            string dept_name = "";
    //            string dept_email_id = "";
    //            string dept_incharge_head_name = "";
    //            SqlDataReader objreader16;
    //            var logincommand16 = new SqlCommand();
    //            logincommand16.CommandText = "SELECT Department_Name,Department_Email_Id,Department_Head_Incharge_Name FROM Department_Division_Master WHERE Department_Id = '" + department_id.ToString() + "' ";
    //            logincommand16.Connection = con;
    //            con.Open();
    //            objreader16 = logincommand16.ExecuteReader();
    //            if (objreader16.Read())
    //            {
    //                dept_name = objreader16["Department_Name"].ToString();
    //                dept_email_id = objreader16["Department_Email_Id"].ToString();
    //                dept_incharge_head_name = objreader16["Department_Head_Incharge_Name"].ToString();
    //            }

    //            con.Close();

    //            // Get Dean Admin Email Id for Approval Notifications
    //            string dean_admin_email_id = "";
    //            SqlDataReader objreader17;
    //            var logincommand17 = new SqlCommand();
    //            logincommand17.CommandText = "SELECT Email_Id FROM Dean_Admin_Mail_Id_For_Approval_Notifications";
    //            logincommand17.Connection = con;
    //            con.Open();
    //            objreader17 = logincommand17.ExecuteReader();
    //            if (objreader17.Read())
    //            {
    //                dean_admin_email_id = objreader17["Email_Id"].ToString();
    //            }

    //            con.Close();



    //            // ----Send Mail to Dean Admin-----
    //            var mail = new MailMessage();
    //            string mail_to = string.Empty;
    //            string cc = string.Empty;
    //            string from_address = string.Empty;
    //            string body = string.Empty;
    //            string subject = string.Empty;
    //            mail_to = dean_admin_email_id.ToString();
    //            from_address = "deanadmin.approvals@pilani.bits-pilani.ac.in";
    //            subject = "Form-A Approval Request (Approval No.:" + " " + approval_number.ToUpper().ToString() + ")" + "";
    //            string approval_status = "Pending";
    //            Session["Mail_Processed_On"] = DateTime.Now.ToString("dd-MMM-yyyy hh:mm tt");
    //            var htmlBuilder = new StringBuilder();
    //            mail.From = new MailAddress(from_address, "BITS Pilani, Pilani Campus");
    //            mail.To.Add(new MailAddress(mail_to));
    //            mail.Subject = subject;
    //            string user_name = "Sir";
    //            try
    //            {
    //                mail.Body = "<html><body><p><b>Dear " + user_name.ToString() + ",</b></p><br>";
    //                mail.Body = mail.Body + "One of the approval request requires your approval.The approval details are as follows:<br><br>";
    //                mail.Body = mail.Body + "<strong>Approval No.:&nbsp;&nbsp;</strong>" + approval_number.ToUpper().ToString() + "<br><br>";
    //                mail.Body = mail.Body + "<strong>Name of the User:&nbsp;&nbsp;</strong>" + lbl_name_of_user.Text.ToString() + "<br><br>";
    //                mail.Body = mail.Body + "<strong>Department/Division/Unit/Centre:&nbsp;&nbsp;</strong>" + lbl_department_name_p.Text.ToString() + "<br><br>";
    //                mail.Body = mail.Body + "<strong>Approval Date :&nbsp;&nbsp;</strong>" + lbl_current_date_p.Text.ToString() + "<br><br>";
    //                mail.Body = mail.Body + "<strong>Request Status:&nbsp;&nbsp;</strong>" + approval_status.ToString() + "<br><br>";
    //                mail.Body = mail.Body + "<strong>Request Date & Time:&nbsp;&nbsp;</strong>" + Session["Mail_Processed_On"].ToString() + "<br><br>";
    //                mail.Body = mail.Body + "<strong>Please visit Budget Information System Portal to view/approve the request.</strong>";
    //                mail.Body = mail.Body + "<br><br>";
    //                mail.Body = mail.Body + "<strong>BITS Pilani, Pilani Campus<strong>";
    //                mail.Body = mail.Body + "<p>-----------------------------------------------------------------------------------------------------------</p>";
    //                mail.Body = mail.Body + "<strong><u>Note:</u>&nbsp;&nbsp;</strong>This is an autogenerated e-mail. Please do not reply to this e-mail.";
    //                mail.IsBodyHtml = true;
    //                mail.Priority = MailPriority.High;
    //                var mSmtpClient = new SmtpClient();
    //                mSmtpClient.Host = "smtp.gmail.com";
    //                mSmtpClient.Port = 587;
    //                mSmtpClient.EnableSsl = true;
    //                mSmtpClient.UseDefaultCredentials = false;
    //                mSmtpClient.Credentials = new System.Net.NetworkCredential(Email_User_ID, Email_Password);
    //                //ServicePointManager.ServerCertificateValidationCallback = AcceptAllCertifications;
    //                mSmtpClient.Send(mail);
    //            }
    //            catch (Exception ex)
    //            {
    //                Response.Write(ex.ToString());
    //            }

    //            Response.Redirect("Institute_Approval_Confirmation.aspx");
    //        }
    //    }
    //}


    //#region Add New ROW
    //protected void grd_Travel_details_RowDeleting(object sender, GridViewDeleteEventArgs e)
    //{
    //    SetRowData();
    //    if (ViewState["CurrentTable"] != null)
    //    {
    //        DataTable dt = (DataTable)ViewState["CurrentTable"];
    //        DataRow drCurrentRow = null;
    //        int rowIndex = Convert.ToInt32(e.RowIndex);
    //        if (dt.Rows.Count > 1)
    //        {
    //            dt.Rows.Remove(dt.Rows[rowIndex]);
    //            drCurrentRow = dt.NewRow();
    //            ViewState["CurrentTable"] = dt;
    //            grd_Travel_details.DataSource = dt;
    //            grd_Travel_details.DataBind();

    //            for (int i = 0; i < grd_Travel_details.Rows.Count - 1; i++)
    //            {
    //                grd_Travel_details.Rows[i].Cells[0].Text = Convert.ToString(i + 1);
    //            }
    //            SetPreviousData();

    //        }
    //        else
    //        {
    //            SetInitialRow();
    //        }
    //    }
    //}
    //protected void grd_Travel_details_RowDataBound(object sender, GridViewRowEventArgs e)
    //{

    //}



    //protected void ButtonAdd_Click(object sender, EventArgs e)
    //{
    //    AddNewRowToGrid();
    //}

    //private void SetInitialRow()
    //{

    //    DataTable dt = new DataTable();
    //    DataRow dr = null;
    //    dt.Columns.Add(new DataColumn("RowNumber", typeof(string)));
    //    dt.Columns.Add(new DataColumn("Description", typeof(string)));
    //    dt.Columns.Add(new DataColumn("Quantity", typeof(string)));
    //    dt.Columns.Add(new DataColumn("Total_Cost", typeof(string)));

    //    dr = dt.NewRow();
    //    dr["RowNumber"] = 1;
    //    dr["Description"] = string.Empty;
    //    dr["Quantity"] = string.Empty;
    //    dr["Total_Cost"] = string.Empty;

    //    dt.Rows.Add(dr);

    //    //Store the DataTable in ViewState
    //    ViewState["CurrentTable"] = dt;

    //    grd_Travel_details.DataSource = dt;
    //    grd_Travel_details.DataBind();
    //}

    //private void AddNewRowToGrid()
    //{
    //    int rowIndex = 0;

    //    DataTable dtCurrentTable = (DataTable)ViewState["CurrentTable"];
    //    DataRow drCurrentRow = null;
    //    if (dtCurrentTable.Rows.Count > 0)
    //    {
    //        for (int i = 1; i <= dtCurrentTable.Rows.Count; i++)
    //        {
    //            TextBox Description = (TextBox)grd_Travel_details.Rows[rowIndex].FindControl("txt_item_description");
    //            TextBox Quantity = (TextBox)grd_Travel_details.Rows[rowIndex].FindControl("txt_item_quantity");
    //            TextBox Total_Cost = (TextBox)grd_Travel_details.Rows[rowIndex].FindControl("txt_item_cost");


    //            drCurrentRow = dtCurrentTable.NewRow();
    //            drCurrentRow["RowNumber"] = i + 1;

    //            dtCurrentTable.Rows[i - 1]["Description"] = Description.Text;
    //            dtCurrentTable.Rows[i - 1]["Quantity"] = Description.Text;
    //            dtCurrentTable.Rows[i - 1]["Total_Cost"] = Description.Text;

    //            rowIndex++;
    //        }
    //        dtCurrentTable.Rows.Add(drCurrentRow);
    //        ViewState["CurrentTable"] = dtCurrentTable;

    //        grd_Travel_details.DataSource = dtCurrentTable;
    //        grd_Travel_details.DataBind();
    //    }

    //    SetPreviousData();
    //}

    //private void SetPreviousData()
    //{
    //    int rowIndex = 0;


    //    DataTable dt = (DataTable)ViewState["CurrentTable"];
    //    if (dt.Rows.Count > 0)
    //    {
    //        for (int i = 0; i < dt.Rows.Count; i++)
    //        {
    //            TextBox Description = (TextBox)grd_Travel_details.Rows[rowIndex].FindControl("txt_item_description");
    //            TextBox Quantity = (TextBox)grd_Travel_details.Rows[rowIndex].FindControl("txt_item_quantity");
    //            TextBox Total_Cost = (TextBox)grd_Travel_details.Rows[rowIndex].FindControl("txt_item_cost");


    //            Description.Text = dt.Rows[i]["Description"].ToString();
    //            Quantity.Text = dt.Rows[i]["Quantity"].ToString();
    //            Total_Cost.Text = dt.Rows[i]["Total_Cost"].ToString();


    //            rowIndex++;
    //        }
    //    }
    //}

    //private void SetRowData()
    //{
    //    int rowIndex = 0;

    //    if (ViewState["CurrentTable"] != null)
    //    {
    //        DataTable dtCurrentTable = (DataTable)ViewState["CurrentTable"];
    //        DataRow drCurrentRow = null;
    //        if (dtCurrentTable.Rows.Count > 0)
    //        {
    //            for (int i = 1; i <= dtCurrentTable.Rows.Count; i++)
    //            {
    //                TextBox Description = (TextBox)grd_Travel_details.Rows[rowIndex].FindControl("txt_item_description");
    //                TextBox Quantity = (TextBox)grd_Travel_details.Rows[rowIndex].FindControl("txt_item_quantity");
    //                TextBox Total_Cost = (TextBox)grd_Travel_details.Rows[rowIndex].FindControl("txt_item_cost");



    //                drCurrentRow = dtCurrentTable.NewRow();
    //                drCurrentRow["RowNumber"] = i + 1;

    //                dtCurrentTable.Rows[i - 1]["Description"] = Description.Text;
    //                dtCurrentTable.Rows[i - 1]["Quantity"] = Description.Text;
    //                dtCurrentTable.Rows[i - 1]["Total_Cost"] = Description.Text;

    //                rowIndex++;
    //            }
    //            //dtCurrentTable.Rows.Add(drCurrentRow);
    //            //ViewState["CurrentTable"] = dtCurrentTable;

    //            //grd_Travel_details.DataSource = dtCurrentTable;
    //            //grd_Travel_details.DataBind();
    //            ViewState["CurrentTable"] = dtCurrentTable;
    //        }
    //    }
    //    else
    //    {
    //        Response.Write("ViewState is null");
    //    }
    //    //SetPreviousData();
    //}
    //#endregion 
    #endregion

    protected void btn_preview_Click(object sender, EventArgs e)
    {
        int Cost = 0;
        int rowIndex = 0;
        Panel1.Enabled = false;
        Panel2.Visible = true;

        justification.InnerHtml = txt_justification.Text;
        txt_justification.Style.Add("display", "none"); ;



        DataTable dt = new DataTable();
        DataRow dr = null;
        dt.Columns.Add(new DataColumn("RowNumber", typeof(string)));
        dt.Columns.Add(new DataColumn("txt_item_description", typeof(string)));
        dt.Columns.Add(new DataColumn("txt_item_quantity", typeof(string)));
        dt.Columns.Add(new DataColumn("txt_item_cost", typeof(string)));



        //DataTable dtCurrentTable = new DataTable();
        //DataRow drCurrentRow = null;
        //if (dtCurrentTable.Rows.Count > 0)
        //{
        for (int i = 0; i < grd_Travel_details.Rows.Count; i++)
        {
            TextBox txt_item_description = (TextBox)grd_Travel_details.Rows[i].FindControl("txt_item_description");
            TextBox txt_item_quantity = (TextBox)grd_Travel_details.Rows[i].FindControl("txt_item_quantity");
            TextBox txt_item_cost = (TextBox)grd_Travel_details.Rows[i].FindControl("txt_item_cost");


            dr = dt.NewRow();
            dr["RowNumber"] = 1;
            dr["txt_item_description"] = txt_item_description.Text;
            dr["txt_item_quantity"] = txt_item_quantity.Text;
            dr["txt_item_cost"] = txt_item_cost.Text;


            dt.Rows.Add(dr);

            //rowIndex++;
        }

        dt.AcceptChanges();
        //ViewState["CurrentTable"] = dtCurrentTable;

        //grd_Travel_details.DataSource = dtCurrentTable;
        //grd_Travel_details.DataBind();
        // }
        //DataTable dt = (DataTable)ViewState["CurrentTable"];
        //SetPreviousData();
        grd_Travel_details.Style.Add("display", "none");
        GridView1.Style.Add("display", "");
        ViewState["CurrentTable"] = dt;
        GridView1.DataSource = dt;
        GridView1.DataBind();

        for (int i = 0; i < GridView1.Rows.Count; i++)
        {
            Label txt_item_description = (Label)GridView1.Rows[i].FindControl("txt_item_description");
            Label txt_item_quantity = (Label)GridView1.Rows[i].FindControl("txt_item_quantity");
            Label txt_item_cost = (Label)GridView1.Rows[i].FindControl("txt_item_cost");




            Cost += int.Parse(txt_item_cost.Text == "" ? "0" : txt_item_cost.Text);


            //rowIndex++;
        }

        Label lbl_footer = (Label)GridView1.FooterRow.FindControl("lbl_footer");
        lbl_footer.Text = Cost.ToString();
        

        btn_preview.Style.Add("display", "none");
    }

    protected void btn_edit_Click(object sender, EventArgs e)
    {
        DataTable dt = new DataTable();
        DataRow dr = null;
        dt.Columns.Add(new DataColumn("RowNumber", typeof(string)));
        dt.Columns.Add(new DataColumn("txt_item_description", typeof(string)));
        dt.Columns.Add(new DataColumn("txt_item_quantity", typeof(string)));
        dt.Columns.Add(new DataColumn("txt_item_cost", typeof(string)));

        for (int i = 0; i < GridView1.Rows.Count; i++)
        {
            Label txt_item_description = (Label)GridView1.Rows[i].FindControl("txt_item_description");
            Label txt_item_quantity = (Label)GridView1.Rows[i].FindControl("txt_item_quantity");
            Label txt_item_cost = (Label)GridView1.Rows[i].FindControl("txt_item_cost");


            dr = dt.NewRow();
            dr["RowNumber"] = 1;
            dr["txt_item_description"] = txt_item_description.Text;
            dr["txt_item_quantity"] = txt_item_quantity.Text;
            dr["txt_item_cost"] = txt_item_cost.Text;


            dt.Rows.Add(dr);

            //rowIndex++;
        }

        dt.AcceptChanges();
        ViewState["CurrentTable"] = dt;

        grd_Travel_details.Style.Add("display", "");
        DataTable dtCurrentTable = (DataTable)ViewState["CurrentTable"];
        grd_Travel_details.DataSource = dtCurrentTable;
        grd_Travel_details.DataBind();
        GridView1.Style.Add("display", "none");

        justification.InnerHtml = "";
        txt_justification.Style.Add("display", ""); ;

        btn_preview.Style.Add("display", "");

        Panel1.Enabled = true;
        Panel2.Visible = false;
    }


    //protected void btn_submit_Click(object sender, EventArgs e)
    //{
    //    if (Session["captcha"].ToString() == txt_captcha_code.Text)
    //    {
    //        decimal TotalCost = 0;

    //        for (int x = 0; x < grd_Travel_details.Rows.Count; x++)
    //        {
    //            TextBox txt_item_description = (TextBox)grd_Travel_details.Rows[x].FindControl("txt_item_description");
    //            TextBox txt_item_quantity = (TextBox)grd_Travel_details.Rows[x].FindControl("txt_item_quantity");
    //            TextBox txt_item_cost = (TextBox)grd_Travel_details.Rows[x].FindControl("txt_item_cost");
    //            TotalCost += Convert.ToDecimal(txt_item_cost.Text == "" ? "0" : txt_item_cost.Text);
    //        }

    //        CC.Getdata("insert into tbl_Individual_Equipment_form(UserName,BudgetHead,ProjectTitle,BudgetHead_BalanceAmount,Justification,Added_By,AddedByName,Added_On,IndividualIP_Address,All_Total_Cost,Department)"
    //            + " values ('" + lbl_user_name.Text.Replace("'", "''") + "','" + lbl_budget_head_details.Text.Replace("'", "''") + "','" + lbl_Project_head_Title.Text.Replace("'", "''") + "',"
    //            + " '" + lbl_budget_head_balance_amount.Text.Replace("'", "''") + "','" + txt_justification.Text.Replace("'", "''") + "','" + Request.QueryString["Session_Id"].ToString() + "','" + Session["Email"].ToString() + "',getdate(),'" + Session["User_IP_Address"].ToString() + "','" + TotalCost + "','" + lbl_department_name.Text + "')");

    //        for (int x = 0; x < grd_Travel_details.Rows.Count; x++)
    //        {
    //            TextBox txt_item_description = (TextBox)grd_Travel_details.Rows[x].FindControl("txt_item_description");
    //            TextBox txt_item_quantity = (TextBox)grd_Travel_details.Rows[x].FindControl("txt_item_quantity");
    //            TextBox txt_item_cost = (TextBox)grd_Travel_details.Rows[x].FindControl("txt_item_cost");


    //            // TotalCost = Convert.ToDecimal(txt_item_cost.Text == "" ? "0" : txt_item_cost.Text);

    //            CC.Getdata("insert into tbl_Individual_Equipment_form_Details(Added_By,ItemDescription,Quantity,ApproxTotalCost,All_Total_Cost)"
    //            + " values ('" + Request.QueryString["Session_Id"].ToString() + "','" + txt_item_description.Text.Replace("'", "''") + "','" + txt_item_quantity.Text.Replace("'", "''") + "','" + txt_item_cost.Text.Replace("'", "''") + "','" + TotalCost + "')");

    //        }

    //        // CC.Getdata("update tbl_Individual_Consumables_form set All_Total_Cost");


    //        string message = "Data Save Successfully.";
    //        string url = "Approved_Budget_Individual.aspx";
    //        string script = "window.onload = function(){ alert('";
    //        script += message;
    //        script += "');";
    //        script += "window.location = '";
    //        script += url;
    //        script += "'; }";
    //        this.ClientScript.RegisterStartupScript(this.GetType(), "Redirect", script, true);
    //    }
    //    else
    //    {
    //        lbl_error_message.Text = "Wrong Captcha Code";
    //    }
    //}


    protected void btn_submit_Click(object sender, EventArgs e)
    {
        if (Session["captcha"].ToString() == txt_captcha_code.Text)
        {
            decimal TotalCost = 0;
            string ApprvalNo = "";
            string Last_Sequence_No = "";
            string Director_Approval_Status = "Pending";

            for (int x = 0; x < grd_Travel_details.Rows.Count; x++)
            {
                TextBox txt_item_description = (TextBox)grd_Travel_details.Rows[x].FindControl("txt_item_description");
                TextBox txt_item_quantity = (TextBox)grd_Travel_details.Rows[x].FindControl("txt_item_quantity");
                TextBox txt_item_cost = (TextBox)grd_Travel_details.Rows[x].FindControl("txt_item_cost");
                TotalCost += Convert.ToDecimal(txt_item_cost.Text == "" ? "0" : txt_item_cost.Text);

            }
            decimal Balance11 = Convert.ToDecimal(lbl_budget_head_balance_amount.Text.Replace("Rs.", "").Trim() == "" ? "0" : lbl_budget_head_balance_amount.Text.Replace("Rs.", "").Trim());

            if (TotalCost <= Balance11)
            {
                DataSet ds21 = new DataSet();
                ds21 = CC.Getdata("select max(Sequence_no) as Last_Sequence_No from tbl_Individual_Consumables_form where Financial_Year='" + Session["Form_A_Entry_Financial_Year"].ToString() + "'");
                if (ds21 != null && ds21.Tables[0].Rows.Count > 0)
                {
                    if (!string.IsNullOrEmpty(ds21.Tables[0].Rows[0]["Last_Sequence_No"].ToString()))
                    {
                        Last_Sequence_No = (int.Parse(ds21.Tables[0].Rows[0]["Last_Sequence_No"].ToString()) + 1).ToString();
                        ApprvalNo = "BITS/PRJ-IND/" + Last_Sequence_No;// (int.Parse(ds21.Tables[0].Rows[0]["Last_Sequence_No"].ToString()) + 1);
                    }
                    else
                    {
                        Last_Sequence_No = "1";
                        ApprvalNo = "BITS/PRJ-IND/" + Last_Sequence_No;// (int.Parse(ds21.Tables[0].Rows[0]["Last_Sequence_No"].ToString()) + 1);
                    }
                }

                if (TotalCost <= 100000)
                {
                    Director_Approval_Status = "Approval Not Required";
                }
                else
                    Director_Approval_Status = "Pending";


                //CC.Getdata("insert into tbl_Individual_Consumables_form(UserName,BudgetHead,ProjectTitle,BudgetHead_BalanceAmount,Justification,Added_By,AddedByName,Added_On,IndividualIP_Address,All_Total_Cost,Estimated_Amount, Department,Approval_No,Financial_Year,SubHead,Sequence_no,Approval_Status,Department_ID,HOD_Approval_Status,SRCD_Dean_Approval_Status,Dean_Approval_Status,Director_Approval_Status)"
                //    + " values ('" + lbl_user_name.Text.Replace("'", "''") + "','" + lbl_budget_head_details.Text.Replace("'", "''") + "','" + lbl_Project_head_Title.Text.Replace("'", "''") + "',"
                //    + " '" + lbl_budget_head_balance_amount.Text.Replace("'", "''") + "','" + txt_justification.Text.Replace("'", "''") + "','" + Request.QueryString["Session_Id"].ToString() + "',"
                //    + "'" + Session["Email"].ToString() + "',getdate(),'" + Session["User_IP_Address"].ToString() + "','" + TotalCost + "','" + TotalCost + "','" + lbl_department_name.Text + "','" + ApprvalNo + "','" + Session["Form_A_Entry_Financial_Year"].ToString() + "','" + Session["Form_A_Entry_Sub_Head"].ToString() + "','" + Last_Sequence_No + "','Pending','" + Session["Department_Divison_Id"].ToString() + "','Pending','Pending','Pending','" + Director_Approval_Status + "')");

                CC.Getdata("insert into tbl_Individual_Consumables_form(UserName,Head,ProjectTitle,BudgetHead_BalanceAmount,Justification,Added_By,AddedByName,Added_On,IndividualIP_Address,All_Total_Cost,Estimated_Amount, Department,Approval_No,Financial_Year,Sub_Head,Sequence_no,Approval_Status,Department_ID,HOD_Approval_Status,SRCD_Dean_Approval_Status,Dean_Approval_Status,Director_Approval_Status,Bill_Amount_Submit_Status,Bill_Amount)"
                 + " values ('" + lbl_user_name.Text.Replace("'", "''") + "','" + Session["Form_A_Entry_Head"].ToString().Replace("'", "''") + "','" + lbl_Project_head_Title.Text.Replace("'", "''") + "',"
                 + " '" + lbl_budget_head_balance_amount.Text.Replace("'", "''") + "','" + txt_justification.Text.Replace("'", "''") + "','" + Request.QueryString["Session_Id"].ToString() + "',"
                 + "'" + Session["Email"].ToString() + "',getdate(),'" + Session["User_IP_Address"].ToString() + "','" + TotalCost + "','" + TotalCost + "','" + lbl_department_name.Text + "','" + ApprvalNo + "','" + Session["Form_A_Entry_Financial_Year"].ToString() + "','" + Session["Form_A_Entry_Sub_Head"].ToString() + "','" + Last_Sequence_No + "','Pending','" + Session["Department_Divison_Id"].ToString() + "','Pending','Pending','Pending','" + Director_Approval_Status + "','Pending','0')");


                for (int x = 0; x < grd_Travel_details.Rows.Count; x++)
                {
                    TextBox txt_item_description = (TextBox)grd_Travel_details.Rows[x].FindControl("txt_item_description");
                    TextBox txt_item_quantity = (TextBox)grd_Travel_details.Rows[x].FindControl("txt_item_quantity");
                    TextBox txt_item_cost = (TextBox)grd_Travel_details.Rows[x].FindControl("txt_item_cost");


                    // TotalCost = Convert.ToDecimal(txt_item_cost.Text == "" ? "0" : txt_item_cost.Text);

                    CC.Getdata("insert into tbl_Individual_Consumables_form_Details(Added_By,ItemDescription,Quantity,ApproxTotalCost,All_Total_Cost,Approval_No,Department_Id,FinancialYear)"
                    + " values ('" + Request.QueryString["Session_Id"].ToString() + "','" + txt_item_description.Text.Replace("'", "''") + "','" + txt_item_quantity.Text.Replace("'", "''") + "','" + txt_item_cost.Text.Replace("'", "''") + "','" + TotalCost + "','" + ApprvalNo + "','" + Session["Department_Divison_Id"].ToString() + "','" + Session["Form_A_Entry_Financial_Year"].ToString() + "')");

                }

                Response.Redirect("Individual_Approval_Confirmation.aspx");
            }
            else
            {
                Page.RegisterStartupScript("as", "<script>alert('Please check your balance amount');</script>");
            }
        }
        else
        {
            lbl_error_message.Visible = true;
            lbl_error_message.Text = "Wrong Captcha Code";
            //lbl_error_message.Text = "Budgeting Cost should be less thne Budget Head Balance Amount";

        }
    }
}