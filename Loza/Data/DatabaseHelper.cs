using Lota.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Windows;

namespace Lota.Data
{
    public class DatabaseHelper
    {
        private const string ConnectionString = "server=localhost;port=3306;database=loza_time_tracking;uid=root;password=;";
        private MySqlConnection GetConnection()
        {
            var conn = new MySqlConnection(ConnectionString);
            conn.Open();
            return conn;
        }
        public static (string salt, string hash) CreatePasswordHash(string password)
        {
            byte[] saltBytes = new byte[32];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(saltBytes);
            }

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 10000, HashAlgorithmName.SHA256))
            {
                byte[] hashBytes = pbkdf2.GetBytes(32);
                string salt = Convert.ToBase64String(saltBytes);
                string hash = Convert.ToBase64String(hashBytes);
                return (salt, hash);
            }
        }
        public static bool VerifyPassword(string password, string storedSalt, string storedHash)
        {
            byte[] saltBytes = Convert.FromBase64String(storedSalt);
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 10000, HashAlgorithmName.SHA256))
            {
                byte[] hashBytes = pbkdf2.GetBytes(32);
                string newHash = Convert.ToBase64String(hashBytes);
                return newHash == storedHash;
            }
        } 
        public bool RegisterUser(string username, string password, string fullName, string email)
        {
            var (salt, hash) = CreatePasswordHash(password);
            using (var conn = GetConnection())
            {
                string query = @"INSERT INTO users (username, password_hash, salt, full_name, email, is_approved) 
                                 VALUES (@un, @ph, @s, @fn, @em, 0)";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@un", username);
                    cmd.Parameters.AddWithValue("@ph", hash);
                    cmd.Parameters.AddWithValue("@s", salt);
                    cmd.Parameters.AddWithValue("@fn", fullName);
                    cmd.Parameters.AddWithValue("@em", email);
                    try
                    {
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                    catch (MySqlException ex) when (ex.Number == 1062)
                    {
                        return false;
                    }
                }
            }
        }
        public (bool success, string message, User user) Authenticate(string username, string password)
        {
            User user = null;
            using (var conn = GetConnection())
            {
                string query = "SELECT * FROM users WHERE username = @un";
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@un", username);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                            return (false, "Пользователь не найден.", null);

                        user = new User
                        {
                            Id = reader.GetInt32("id"),
                            Username = reader.GetString("username"),
                            PasswordHash = reader.GetString("password_hash"),
                            Salt = reader.GetString("salt"),
                            FullName = reader.GetString("full_name"),
                            Email = reader.GetString("email"),
                            Role = reader.GetString("role"),
                            IsApproved = reader.GetBoolean("is_approved")
                        };
                    }
                }
            }
            if (!VerifyPassword(password, user.Salt, user.PasswordHash))
                return (false, "Неверный пароль.", null);
            if (!user.IsApproved)
                return (false, "Учётная запись ожидает подтверждения администратором.", null);

            return (true, "Авторизация успешна.", user);
        }
        public List<Employee> GetAllEmployees()
        {
            var employees = new List<Employee>();
            using (var conn = GetConnection())
            {
                string sql = "SELECT * FROM employees ORDER BY last_name";
                using (var cmd = new MySqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        employees.Add(new Employee
                        {
                            Id = reader.GetInt32("id"),
                            UserId = reader.IsDBNull(reader.GetOrdinal("user_id")) ? (int?)null : reader.GetInt32("user_id"),
                            FirstName = reader.GetString("first_name"),
                            LastName = reader.GetString("last_name"),
                            MiddleName = reader.IsDBNull(reader.GetOrdinal("middle_name")) ? null : reader.GetString("middle_name"),
                            Position = reader.GetString("position"),
                            Department = reader.GetString("department"),
                            HireDate = reader.GetDateTime("hire_date"),
                            Status = reader.GetString("status")
                        });
                    }
                }
            }
            return employees;
        }

        public bool AddEmployee(Employee emp)
        {
            using (var conn = GetConnection())
            {
                string sql = @"INSERT INTO employees (user_id, first_name, last_name, middle_name, position, department, hire_date, status)
                       VALUES (@uid, @fn, @ln, @mn, @pos, @dep, @hd, @stat)";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@uid", (object)emp.UserId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@fn", emp.FirstName);
                    cmd.Parameters.AddWithValue("@ln", emp.LastName);
                    cmd.Parameters.AddWithValue("@mn", (object)emp.MiddleName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@pos", emp.Position);
                    cmd.Parameters.AddWithValue("@dep", emp.Department);
                    cmd.Parameters.AddWithValue("@hd", emp.HireDate);
                    cmd.Parameters.AddWithValue("@stat", emp.Status);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
        public bool UpdateEmployee(Employee emp)
        {
            using (var conn = GetConnection())
            {
                string sql = @"UPDATE employees SET 
                        user_id = @uid,
                        first_name = @fn,
                        last_name = @ln,
                        middle_name = @mn,
                        position = @pos,
                        department = @dep,
                        hire_date = @hd,
                        status = @stat
                       WHERE id = @id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", emp.Id);
                    cmd.Parameters.AddWithValue("@uid", (object)emp.UserId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@fn", emp.FirstName);
                    cmd.Parameters.AddWithValue("@ln", emp.LastName);
                    cmd.Parameters.AddWithValue("@mn", (object)emp.MiddleName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@pos", emp.Position);
                    cmd.Parameters.AddWithValue("@dep", emp.Department);
                    cmd.Parameters.AddWithValue("@hd", emp.HireDate);
                    cmd.Parameters.AddWithValue("@stat", emp.Status);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
        public bool DeleteEmployee(int id)
        {
            using (var conn = GetConnection())
            {
                string sql = "DELETE FROM employees WHERE id = @id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
        public List<TimeLogRecord> GetTimeLogs(DateTime? dateFrom = null, DateTime? dateTo = null, int? employeeId = null)
        {
            var records = new List<TimeLogRecord>();
            using (var conn = GetConnection())
            {
                string sql = @"SELECT tl.id, tl.employee_id, tl.login_time, tl.logout_time, tl.notes,
                              CONCAT(e.last_name, ' ', e.first_name, ' ', COALESCE(e.middle_name, '')) AS employee_full_name
                       FROM time_log tl
                       JOIN employees e ON tl.employee_id = e.id
                       WHERE 1=1";
                if (dateFrom.HasValue)
                    sql += " AND tl.login_time >= @from";
                if (dateTo.HasValue)
                    sql += " AND tl.login_time <= @to";
                if (employeeId.HasValue)
                    sql += " AND tl.employee_id = @empId";
                sql += " ORDER BY tl.login_time DESC";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    if (dateFrom.HasValue) cmd.Parameters.AddWithValue("@from", dateFrom.Value);
                    if (dateTo.HasValue) cmd.Parameters.AddWithValue("@to", dateTo.Value);
                    if (employeeId.HasValue) cmd.Parameters.AddWithValue("@empId", employeeId.Value);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            records.Add(new TimeLogRecord
                            {
                                Id = reader.GetInt32("id"),
                                EmployeeId = reader.GetInt32("employee_id"),
                                LoginTime = reader.GetDateTime("login_time"),
                                LogoutTime = reader.IsDBNull(reader.GetOrdinal("logout_time")) ? (DateTime?)null : reader.GetDateTime("logout_time"),
                                Notes = reader.IsDBNull(reader.GetOrdinal("notes")) ? null : reader.GetString("notes"),
                                EmployeeFullName = reader.GetString("employee_full_name")
                            });
                        }
                    }
                }
            }
            return records;
        }

        public bool AddTimeLog(TimeLogRecord record)
        {
            using (var conn = GetConnection())
            {
                string sql = "INSERT INTO time_log (employee_id, login_time, logout_time, notes) VALUES (@eid, @lt, @lo, @n)";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@eid", record.EmployeeId);
                    cmd.Parameters.AddWithValue("@lt", record.LoginTime);
                    cmd.Parameters.AddWithValue("@lo", (object)record.LogoutTime ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@n", (object)record.Notes ?? DBNull.Value);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool UpdateTimeLog(TimeLogRecord record)
        {
            using (var conn = GetConnection())
            {
                string sql = "UPDATE time_log SET employee_id = @eid, login_time = @lt, logout_time = @lo, notes = @n WHERE id = @id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", record.Id);
                    cmd.Parameters.AddWithValue("@eid", record.EmployeeId);
                    cmd.Parameters.AddWithValue("@lt", record.LoginTime);
                    cmd.Parameters.AddWithValue("@lo", (object)record.LogoutTime ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@n", (object)record.Notes ?? DBNull.Value);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool DeleteTimeLog(int id)
        {
            using (var conn = GetConnection())
            {
                string sql = "DELETE FROM time_log WHERE id = @id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
        public List<TimesheetRow> GetTimesheetData(int year, int month)
        {
            var rows = new List<TimesheetRow>();
            int daysInMonth = DateTime.DaysInMonth(year, month);
            DateTime monthStart = new DateTime(year, month, 1);
            DateTime monthEnd = monthStart.AddMonths(1).AddSeconds(-1);

            var employees = GetAllEmployees().Where(e => e.Status == "active").ToList();

            using (var conn = GetConnection())
            {
                string sql = @"SELECT 
                        tl.employee_id,
                        DATE(tl.login_time) AS work_date,
                        SUM(TIMESTAMPDIFF(MINUTE, tl.login_time, COALESCE(tl.logout_time, tl.login_time))) AS total_minutes
                    FROM time_log tl
                    WHERE tl.login_time BETWEEN @start AND @end
                    GROUP BY tl.employee_id, DATE(tl.login_time)
                    ORDER BY tl.employee_id, work_date";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@start", monthStart);
                    cmd.Parameters.AddWithValue("@end", monthEnd);
                    var dayData = new Dictionary<int, Dictionary<int, decimal>>();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int empId = reader.GetInt32("employee_id");
                            DateTime date = reader.GetDateTime("work_date");
                            int day = date.Day;
                            decimal minutes = reader.GetDecimal("total_minutes");
                            decimal hours = Math.Round(minutes / 60, 1);

                            if (!dayData.ContainsKey(empId))
                                dayData[empId] = new Dictionary<int, decimal>();
                            if (dayData[empId].ContainsKey(day))
                                dayData[empId][day] += hours;
                            else
                                dayData[empId][day] = hours;
                        }
                    }

                    foreach (var emp in employees)
                    {
                        var row = new TimesheetRow
                        {
                            EmployeeId = emp.Id,
                            FullName = emp.FullName,
                            Position = emp.Position,
                            PersonnelNumber = emp.Id.ToString()
                        };

                        decimal total = 0, half1 = 0, half2 = 0;
                        for (int day = 1; day <= daysInMonth; day++)
                        {
                            if (dayData.ContainsKey(emp.Id) && dayData[emp.Id].ContainsKey(day))
                            {
                                decimal h = dayData[emp.Id][day];
                                row.DayHours[day] = h;
                                total += h;
                                if (day <= 15) half1 += h;
                                else half2 += h;
                            }
                            else
                            {
                                row.DayHours[day] = null;
                            }
                        }
                        row.TotalHours = total;
                        row.Half1Hours = half1;
                        row.Half2Hours = half2;
                        rows.Add(row);
                    }
                }
            }
            return rows;
        }

        public List<User> GetUsersByStatus(bool? isApproved = null)
        {
            var users = new List<User>();
            using (var conn = GetConnection())
            {
                string sql = "SELECT * FROM users";
                if (isApproved.HasValue)
                    sql += " WHERE is_approved = @approved";
                sql += " ORDER BY created_at DESC";

                using (var cmd = new MySqlCommand(sql, conn))
                {
                    if (isApproved.HasValue)
                        cmd.Parameters.AddWithValue("@approved", isApproved.Value);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(new User
                            {
                                Id = reader.GetInt32("id"),
                                Username = reader.GetString("username"),
                                FullName = reader.GetString("full_name"),
                                Email = reader.GetString("email"),
                                Role = reader.GetString("role"),
                                IsApproved = reader.GetBoolean("is_approved"),
                                CreatedAt = reader.GetDateTime("created_at")
                            });
                        }
                    }
                }
            }
            return users;
        }
        public bool ApproveUser(int userId)
        {
            return UpdateUserApproval(userId, true);
        }

        public bool RejectUser(int userId)
        {
            return DeleteUser(userId);
        }

        private bool UpdateUserApproval(int userId, bool isApproved)
        {
            using (var conn = GetConnection())
            {
                string sql = "UPDATE users SET is_approved = @approved WHERE id = @id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@approved", isApproved);
                    cmd.Parameters.AddWithValue("@id", userId);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
        public bool UpdateUser(User user)
        {
            using (var conn = GetConnection())
            {
                string sql = @"UPDATE users 
                       SET username = @un, full_name = @fn, email = @em, role = @role, is_approved = @approved
                       WHERE id = @id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@un", user.Username);
                    cmd.Parameters.AddWithValue("@fn", user.FullName);
                    cmd.Parameters.AddWithValue("@em", user.Email);
                    cmd.Parameters.AddWithValue("@role", user.Role);
                    cmd.Parameters.AddWithValue("@approved", user.IsApproved);
                    cmd.Parameters.AddWithValue("@id", user.Id);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool ChangeUserPassword(int userId, string newPassword)
        {
            var (salt, hash) = CreatePasswordHash(newPassword);
            using (var conn = GetConnection())
            {
                string sql = "UPDATE users SET password_hash = @hash, salt = @salt WHERE id = @id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@hash", hash);
                    cmd.Parameters.AddWithValue("@salt", salt);
                    cmd.Parameters.AddWithValue("@id", userId);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
        private bool DeleteUser(int userId)
        {
            using (var conn = GetConnection())
            {
                string sql = "DELETE FROM users WHERE id = @id";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", userId);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
        public int GetPendingUsersCount()
        {
            using (var conn = GetConnection())
            {
                string sql = "SELECT COUNT(*) FROM users WHERE is_approved = 0";
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }
    }
}