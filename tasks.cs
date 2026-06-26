using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace demo
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string DueDate { get; set; }
        public string Reminder { get; set; }
        public string Status { get; set; }
    }
    public class tasks
    {
        // global connection string - using master database first
        string connection = @"Server=(LocalDB)\MSSQLLocalDB;Integrated Security=True;";
        string databaseName = "CybersecurityTasksDB";
        string tasksDatabaseConnection = string.Empty;

        // AUTO CREATE TABLE METHOD
        public void CreateTableIfNotExists()
        {
            try
            {
                using (SqlConnection masterConnect = new SqlConnection(connection))
                {
                    masterConnect.Open();
                    string checkDbQuery = $@"
                        IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = '{databaseName}')
                        BEGIN
                            CREATE DATABASE {databaseName}
                        END";
                    using (SqlCommand createDbCommand = new SqlCommand(checkDbQuery, masterConnect))
                    {
                        createDbCommand.ExecuteNonQuery();
                    }
                    masterConnect.Close();
                }

                tasksDatabaseConnection = $@"Server=(LocalDB)\MSSQLLocalDB;Database={databaseName};Integrated Security=True;";

                using (SqlConnection connect = new SqlConnection(tasksDatabaseConnection))
                {
                    connect.Open();
                    string createTableQuery = @"
                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='demo_tasks')
                        BEGIN
                            CREATE TABLE demo_tasks (
                                task_id INT IDENTITY(1,1) PRIMARY KEY,
                                task_name NVARCHAR(100) NOT NULL,
                                task_description NVARCHAR(255),
                                task_dueDate NVARCHAR(50),
                                task_reminderDate DATETIME,
                                task_status NVARCHAR(20)
                            )
                        END";
                    using (SqlCommand createCommand = new SqlCommand(createTableQuery, connect))
                    {
                        createCommand.ExecuteNonQuery();
                    }
                    connect.Close();
                }
            }
            catch (Exception error)
            {
                MessageBox.Show("Error creating database/table: " + error.Message);
            }
        }

        public void test_connection()
        {
            try
            {
                using (SqlConnection connect = new SqlConnection(tasksDatabaseConnection))
                {
                    connect.Open();
                    MessageBox.Show("Database connected successfully!");
                    connect.Close();
                }
            }
            catch (Exception error)
            {
                MessageBox.Show("Connection error: " + error.Message);
            }
        }

        // Overload: insert without reminder
        public void insert_task(string name, string description, string dueDate, string status)
        {
            insert_task(name, description, dueDate, status, null);
        }

        // Insert with optional reminder
        public void insert_task(string name, string description, string dueDate, string status, DateTime? reminderDate)
        {
            using (SqlConnection connects = new SqlConnection(tasksDatabaseConnection))
            {
                try
                {
                    connects.Open();
                    string query = @"INSERT INTO demo_tasks (task_name, task_description, task_dueDate, task_reminderDate, task_status) 
                                      VALUES (@name, @description, @dueDate, @reminderDate, @status)";
                    using (SqlCommand run_query = new SqlCommand(query, connects))
                    {
                        run_query.Parameters.AddWithValue("@name", name ?? (object)DBNull.Value);
                        run_query.Parameters.AddWithValue("@description", description ?? (object)DBNull.Value);
                        run_query.Parameters.AddWithValue("@dueDate", dueDate ?? (object)DBNull.Value);
                        run_query.Parameters.AddWithValue("@reminderDate", reminderDate ?? (object)DBNull.Value);
                        run_query.Parameters.AddWithValue("@status", status ?? (object)DBNull.Value);
                        run_query.ExecuteNonQuery();
                    }
                    connects.Close();
                }
                catch (Exception error)
                {
                    MessageBox.Show("Error inserting task: " + error.Message);
                }
            }
        }

        // NEW: Update a task's reminder by its name (for the most recent task without a reminder)
        public void update_taskReminder(string taskName, string dueDate, DateTime reminderDate)
        {
            using (SqlConnection connects = new SqlConnection(tasksDatabaseConnection))
            {
                try
                {
                    connects.Open();
                    string query = @"UPDATE demo_tasks 
                                     SET task_dueDate = @dueDate, task_reminderDate = @reminderDate 
                                     WHERE task_name = @name AND task_reminderDate IS NULL";
                    using (SqlCommand cmd = new SqlCommand(query, connects))
                    {
                        cmd.Parameters.AddWithValue("@name", taskName);
                        cmd.Parameters.AddWithValue("@dueDate", dueDate);
                        cmd.Parameters.AddWithValue("@reminderDate", reminderDate);
                        cmd.ExecuteNonQuery();
                    }
                    connects.Close();
                }
                catch (Exception error)
                {
                    MessageBox.Show("Error updating reminder: " + error.Message);
                }
            }
        }
        // Load tasks into an ObservableCollection for WPF data binding
        public ObservableCollection<TaskItem> LoadTasksAsCollection()
        {
            var collection = new ObservableCollection<TaskItem>();
            using (SqlConnection connects = new SqlConnection(tasksDatabaseConnection))
            {
                try
                {
                    connects.Open();
                    string query = "SELECT task_id, task_name, task_description, task_dueDate, task_reminderDate, task_status FROM demo_tasks";
                    using (SqlCommand cmd = new SqlCommand(query, connects))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var item = new TaskItem
                            {
                                Id = Convert.ToInt32(reader["task_id"]),
                                Title = reader["task_name"].ToString(),
                                Description = reader["task_description"].ToString(),
                                DueDate = reader["task_dueDate"]?.ToString() ?? "N/A",
                                Reminder = reader["task_reminderDate"] != DBNull.Value
                                    ? Convert.ToDateTime(reader["task_reminderDate"]).ToString("yyyy-MM-dd")
                                    : "None",
                                Status = reader["task_status"].ToString()
                            };
                            collection.Add(item);
                        }
                    }
                    connects.Close();
                }
                catch (Exception error)
                {
                    MessageBox.Show("Error loading tasks: " + error.Message);
                }
            }
            return collection;
        }

        public void load_tasks(ListView view_task)
        {
            using (SqlConnection connects = new SqlConnection(tasksDatabaseConnection))
            {
                try
                {
                    connects.Open();
                    string query = $"select * from demo_tasks;";
                    using (SqlCommand run_query = new SqlCommand(query, connects))
                    {
                        using (SqlDataReader data_collect = run_query.ExecuteReader())
                        {
                            bool data_found = false;
                            while (data_collect.Read())
                            {
                                data_found = true;
                                string task_id = data_collect["task_id"].ToString();
                                string task_name = data_collect["task_name"].ToString();
                                string task_description = data_collect["task_description"].ToString();
                                string task_dueDate = data_collect["task_dueDate"].ToString();
                                string task_reminderDate = data_collect["task_reminderDate"].ToString();
                                string task_status = data_collect["task_status"].ToString();
                                string reminderInfo = string.IsNullOrEmpty(task_reminderDate) ? "no reminder" : "reminder on " + task_reminderDate;
                                view_task.Items.Add(task_id + " " + task_name + " with " + task_description + " due on " + task_dueDate + " (" + reminderInfo + ") and is " + task_status);
                            }
                            if (!data_found)
                                view_task.Items.Add("No task found");
                        }
                    }
                    connects.Close();
                }
                catch (Exception error)
                {
                    MessageBox.Show("Error loading tasks: " + error.Message);
                }
            }
        }

        public void update_taskStatus(int id)
        {
            using (SqlConnection connects = new SqlConnection(tasksDatabaseConnection))
            {
                try
                {
                    connects.Open();
                    string query = $"update demo_tasks set task_status='done' where task_id=@id";
                    using (SqlCommand run_query = new SqlCommand(query, connects))
                    {
                        run_query.Parameters.AddWithValue("@id", id);
                        run_query.ExecuteNonQuery();
                    }
                    connects.Close();
                }
                catch (Exception error)
                {
                    MessageBox.Show("Error updating task: " + error.Message);
                }
            }
        }

        public void delete_task(int id)
        {
            using (SqlConnection connects = new SqlConnection(tasksDatabaseConnection))
            {
                try
                {
                    connects.Open();
                    string query = $"delete from demo_tasks where task_id=@id";
                    using (SqlCommand run_query = new SqlCommand(query, connects))
                    {
                        run_query.Parameters.AddWithValue("@id", id);
                        run_query.ExecuteNonQuery();
                    }
                    connects.Close();
                }
                catch (Exception error)
                {
                    MessageBox.Show("Error deleting task: " + error.Message);
                }
            }
        }
    }
}