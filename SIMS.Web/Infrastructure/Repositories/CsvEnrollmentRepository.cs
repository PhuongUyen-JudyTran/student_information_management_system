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
    public class CsvEnrollmentRepository : IEnrollmentRepository
    {
        private const string Filename = "enrollments.csv";
        private readonly CsvContext _context;

        public CsvEnrollmentRepository(CsvContext context)
        {
            _context = context;
        }

        public async Task<List<Enrollment>> GetByStudentIdAsync(string studentId)
        {
            var enrollments = new List<Enrollment>();
            if (string.IsNullOrWhiteSpace(studentId)) return enrollments;

            var lines = await _context.ReadLinesAsync(Filename);
            foreach (var line in lines)
            {
                var fields = CsvHelper.ParseLine(line);
                if (fields.Count < 4) continue;

                if (string.Equals(fields[0], studentId, StringComparison.OrdinalIgnoreCase))
                {
                    enrollments.Add(MapEnrollment(fields));
                }
            }

            return enrollments;
        }

        public async Task<List<Enrollment>> GetByCourseIdAsync(string courseId)
        {
            var enrollments = new List<Enrollment>();
            if (string.IsNullOrWhiteSpace(courseId)) return enrollments;

            var lines = await _context.ReadLinesAsync(Filename);
            foreach (var line in lines)
            {
                var fields = CsvHelper.ParseLine(line);
                if (fields.Count < 4) continue;

                if (string.Equals(fields[1], courseId, StringComparison.OrdinalIgnoreCase))
                {
                    enrollments.Add(MapEnrollment(fields));
                }
            }

            return enrollments;
        }

        public async Task<bool> AddAsync(Enrollment enrollment)
        {
            if (enrollment == null) throw new ArgumentNullException(nameof(enrollment));

            var lines = await _context.ReadLinesAsync(Filename);

            foreach (var line in lines)
            {
                var fields = CsvHelper.ParseLine(line);
                if (fields.Count >= 2 &&
                    string.Equals(fields[0], enrollment.StudentId, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(fields[1], enrollment.CourseId, StringComparison.OrdinalIgnoreCase))
                {
                    return false; // Already enrolled
                }
            }

            var newLine = FormatEnrollment(enrollment);
            lines.Add(newLine);

            await _context.WriteAllLinesAsync(Filename, lines);
            return true;
        }

        public async Task<bool> DeleteAsync(string studentId, string courseId)
        {
            if (string.IsNullOrWhiteSpace(studentId) || string.IsNullOrWhiteSpace(courseId)) return false;

            var lines = await _context.ReadLinesAsync(Filename);
            int indexToDelete = -1;

            for (int i = 0; i < lines.Count; i++)
            {
                var fields = CsvHelper.ParseLine(lines[i]);
                if (fields.Count >= 2 &&
                    string.Equals(fields[0], studentId, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(fields[1], courseId, StringComparison.OrdinalIgnoreCase))
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

        private Enrollment MapEnrollment(List<string> fields)
        {
            return new Enrollment
            {
                StudentId = fields[0],
                CourseId = fields[1],
                EnrollmentDate = DateTime.TryParse(fields[2], CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? date : default,
                Grade = fields[3]
            };
        }

        private string FormatEnrollment(Enrollment enrollment)
        {
            var fields = new string[4];
            fields[0] = enrollment.StudentId;
            fields[1] = enrollment.CourseId;
            fields[2] = enrollment.EnrollmentDate.ToString("o", CultureInfo.InvariantCulture);
            fields[3] = enrollment.Grade;

            return CsvHelper.FormatLine(fields);
        }
    }
}
