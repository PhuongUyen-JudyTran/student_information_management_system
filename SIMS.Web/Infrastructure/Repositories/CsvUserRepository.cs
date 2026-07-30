using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SIMS.Domain.Entities;
using SIMS.Domain.Interfaces;
using SIMS.Infrastructure.Data;

namespace SIMS.Infrastructure.Repositories
{
    public class CsvUserRepository : IUserRepository
    {
        private const string Filename = "users.csv";
        private readonly CsvContext _context;

        public CsvUserRepository(CsvContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return null;

            var lines = await _context.ReadLinesAsync(Filename);
            foreach (var line in lines)
            {
                var fields = CsvHelper.ParseLine(line);
                if (fields.Count < 5) continue;

                if (string.Equals(fields[1], username, StringComparison.OrdinalIgnoreCase))
                {
                    return MapUser(fields);
                }
            }

            return null;
        }

        public async Task<bool> AddAsync(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var lines = await _context.ReadLinesAsync(Filename);

            foreach (var line in lines)
            {
                var fields = CsvHelper.ParseLine(line);
                if (fields.Count >= 2 && (string.Equals(fields[0], user.Id, StringComparison.OrdinalIgnoreCase) ||
                                          string.Equals(fields[1], user.Username, StringComparison.OrdinalIgnoreCase)))
                {
                    return false;
                }
            }

            var newLine = FormatUser(user);
            lines.Add(newLine);

            await _context.WriteAllLinesAsync(Filename, lines);
            return true;
        }

        public async Task<bool> ValidateCredentialsAsync(string username, string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(passwordHash))
                return false;

            var user = await GetByUsernameAsync(username);
            if (user == null) return false;

            return string.Equals(user.PasswordHash, passwordHash, StringComparison.Ordinal);
        }

        public async Task<List<User>> GetAllAsync()
        {
            var users = new List<User>();
            var lines = await _context.ReadLinesAsync(Filename);
            foreach (var line in lines)
            {
                var fields = CsvHelper.ParseLine(line);
                if (fields.Count < 5) continue;
                try
                {
                    users.Add(MapUser(fields));
                }
                catch
                {
                    // skip malformed entries
                }
            }
            return users;
        }

        public async Task<bool> UpdateAsync(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var lines = await _context.ReadLinesAsync(Filename);
            bool updated = false;

            for (int i = 0; i < lines.Count; i++)
            {
                var fields = CsvHelper.ParseLine(lines[i]);
                if (fields.Count >= 5 && string.Equals(fields[0], user.Id, StringComparison.OrdinalIgnoreCase))
                {
                    lines[i] = FormatUser(user);
                    updated = true;
                    break;
                }
            }

            if (!updated) return false;

            await _context.WriteAllLinesAsync(Filename, lines);
            return true;
        }

        public static User MapUser(List<string> fields)
        {
            var id = fields[0];
            var username = fields[1];
            var passwordHash = fields[2];
            var email = fields[3];
            var roleStr = fields[4];

            if (!Enum.TryParse<UserRole>(roleStr, out var role))
            {
                throw new InvalidDataException($"Invalid user role: {roleStr}");
            }

            User user;
            switch (role)
            {
                case UserRole.Student:
                    var dobStr = fields.Count > 5 ? fields[5] : string.Empty;
                    var prog = fields.Count > 6 ? fields[6] : string.Empty;
                    user = new Student
                    {
                        DateOfBirth = DateTime.TryParse(dobStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dob) ? dob : default,
                        Program = prog
                    };
                    break;

                case UserRole.Faculty:
                    var dept = fields.Count > 7 ? fields[7] : string.Empty;
                    user = new Faculty
                    {
                        Department = dept
                    };
                    break;

                case UserRole.Administrator:
                    var adminLevel = fields.Count > 8 ? fields[8] : string.Empty;
                    user = new Administrator
                    {
                        AdminLevel = adminLevel
                    };
                    break;

                default:
                    throw new NotSupportedException($"Role {role} is not supported.");
            }

            user.Id = id;
            user.Username = username;
            user.PasswordHash = passwordHash;
            user.Email = email;

            return user;
        }

        public static string FormatUser(User user)
        {
            var fields = new string[9];
            fields[0] = user.Id;
            fields[1] = user.Username;
            fields[2] = user.PasswordHash;
            fields[3] = user.Email;
            fields[4] = user.Role.ToString();
            fields[5] = string.Empty;
            fields[6] = string.Empty;
            fields[7] = string.Empty;
            fields[8] = string.Empty;

            if (user is Student student)
            {
                fields[5] = student.DateOfBirth.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                fields[6] = student.Program;
            }
            else if (user is Faculty faculty)
            {
                fields[7] = faculty.Department;
            }
            else if (user is Administrator admin)
            {
                fields[8] = admin.AdminLevel;
            }

            return CsvHelper.FormatLine(fields);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return false;

            var lines = await _context.ReadLinesAsync(Filename);
            int indexToDelete = -1;

            for (int i = 0; i < lines.Count; i++)
            {
                var fields = CsvHelper.ParseLine(lines[i]);
                if (fields.Count >= 1 && string.Equals(fields[0], id, StringComparison.OrdinalIgnoreCase))
                {
                    indexToDelete = i;
                    break;
                }
            }

            if (indexToDelete == -1) return false;

            lines.RemoveAt(indexToDelete);
            await _context.WriteAllLinesAsync(Filename, lines);
            return true;
        }
    }
}
