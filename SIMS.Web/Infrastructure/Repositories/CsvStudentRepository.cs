using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SIMS.Domain.Entities;
using SIMS.Domain.Interfaces;
using SIMS.Infrastructure.Data;

namespace SIMS.Infrastructure.Repositories
{
    public class CsvStudentRepository : IStudentRepository
    {
        private const string Filename = "users.csv";
        private readonly CsvContext _context;

        public CsvStudentRepository(CsvContext context)
        {
            _context = context;
        }

        public async Task<Student?> GetByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;

            var lines = await _context.ReadLinesAsync(Filename);
            foreach (var line in lines)
            {
                var fields = CsvHelper.ParseLine(line);
                if (fields.Count < 5) continue;

                if (string.Equals(fields[0], id, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(fields[4], UserRole.Student.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    return CsvUserRepository.MapUser(fields) as Student;
                }
            }

            return null;
        }

        public async Task<List<Student>> GetAllAsync()
        {
            var students = new List<Student>();
            var lines = await _context.ReadLinesAsync(Filename);

            foreach (var line in lines)
            {
                var fields = CsvHelper.ParseLine(line);
                if (fields.Count < 5) continue;

                if (string.Equals(fields[4], UserRole.Student.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    if (CsvUserRepository.MapUser(fields) is Student student)
                    {
                        students.Add(student);
                    }
                }
            }

            return students;
        }

        public async Task<bool> AddAsync(Student student)
        {
            if (student == null) throw new ArgumentNullException(nameof(student));

            var lines = await _context.ReadLinesAsync(Filename);

            foreach (var line in lines)
            {
                var fields = CsvHelper.ParseLine(line);
                if (fields.Count >= 2 && (string.Equals(fields[0], student.Id, StringComparison.OrdinalIgnoreCase) ||
                                          string.Equals(fields[1], student.Username, StringComparison.OrdinalIgnoreCase)))
                {
                    return false; // ID or Username already exists
                }
            }

            var newLine = CsvUserRepository.FormatUser(student);
            lines.Add(newLine);

            await _context.WriteAllLinesAsync(Filename, lines);
            return true;
        }

        public async Task<bool> UpdateAsync(Student student)
        {
            if (student == null) throw new ArgumentNullException(nameof(student));

            var lines = await _context.ReadLinesAsync(Filename);
            bool updated = false;

            for (int i = 0; i < lines.Count; i++)
            {
                var fields = CsvHelper.ParseLine(lines[i]);
                if (fields.Count >= 5 && string.Equals(fields[0], student.Id, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(fields[4], UserRole.Student.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    lines[i] = CsvUserRepository.FormatUser(student);
                    updated = true;
                    break;
                }
            }

            if (!updated) return false;

            await _context.WriteAllLinesAsync(Filename, lines);
            return true;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return false;

            var lines = await _context.ReadLinesAsync(Filename);
            int indexToDelete = -1;

            for (int i = 0; i < lines.Count; i++)
            {
                var fields = CsvHelper.ParseLine(lines[i]);
                if (fields.Count >= 5 && string.Equals(fields[0], id, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(fields[4], UserRole.Student.ToString(), StringComparison.OrdinalIgnoreCase))
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
