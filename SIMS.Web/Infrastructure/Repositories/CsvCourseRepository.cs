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
    public class CsvCourseRepository : ICourseRepository
    {
        private const string Filename = "courses.csv";
        private readonly CsvContext _context;

        public CsvCourseRepository(CsvContext context)
        {
            _context = context;
        }

        public async Task<Course?> GetByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return null;

            var lines = await _context.ReadLinesAsync(Filename);
            foreach (var line in lines)
            {
                var fields = CsvHelper.ParseLine(line);
                if (fields.Count < 4) continue;

                if (string.Equals(fields[0], id, StringComparison.OrdinalIgnoreCase))
                {
                    return MapCourse(fields);
                }
            }

            return null;
        }

        public async Task<List<Course>> GetAllAsync()
        {
            var courses = new List<Course>();
            var lines = await _context.ReadLinesAsync(Filename);

            foreach (var line in lines)
            {
                var fields = CsvHelper.ParseLine(line);
                if (fields.Count < 4) continue;

                courses.Add(MapCourse(fields));
            }

            return courses;
        }

        public async Task<bool> AddAsync(Course course)
        {
            if (course == null) throw new ArgumentNullException(nameof(course));

            var lines = await _context.ReadLinesAsync(Filename);

            foreach (var line in lines)
            {
                var fields = CsvHelper.ParseLine(line);
                if (fields.Count >= 1 && string.Equals(fields[0], course.CourseId, StringComparison.OrdinalIgnoreCase))
                {
                    return false; // Course ID already exists
                }
            }

            var newLine = FormatCourse(course);
            lines.Add(newLine);

            await _context.WriteAllLinesAsync(Filename, lines);
            return true;
        }

        public async Task<bool> UpdateAsync(Course course)
        {
            if (course == null) throw new ArgumentNullException(nameof(course));

            var lines = await _context.ReadLinesAsync(Filename);
            bool updated = false;

            for (int i = 0; i < lines.Count; i++)
            {
                var fields = CsvHelper.ParseLine(lines[i]);
                if (fields.Count >= 1 && string.Equals(fields[0], course.CourseId, StringComparison.OrdinalIgnoreCase))
                {
                    lines[i] = FormatCourse(course);
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

        private Course MapCourse(List<string> fields)
        {
            var credits = int.TryParse(fields[2], out int c) ? c : 0;
            var capacity = int.TryParse(fields[3], out int cap) ? cap : 0;
            var facultyId = fields.Count > 4 ? fields[4] : string.Empty;
            var fee = (fields.Count > 5 && decimal.TryParse(fields[5], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal f))
                ? f
                : credits * 500000m; // Default 500,000 VND / credit if not specified

            return new Course
            {
                CourseId = fields[0],
                Title = fields[1],
                Credits = credits,
                MaxCapacity = capacity,
                FacultyId = facultyId,
                Fee = fee
            };
        }

        private string FormatCourse(Course course)
        {
            var fields = new string[6];
            fields[0] = course.CourseId;
            fields[1] = course.Title;
            fields[2] = course.Credits.ToString(CultureInfo.InvariantCulture);
            fields[3] = course.MaxCapacity.ToString(CultureInfo.InvariantCulture);
            fields[4] = course.FacultyId ?? string.Empty;
            fields[5] = course.Fee.ToString("F2", CultureInfo.InvariantCulture);

            return CsvHelper.FormatLine(fields);
        }
    }
}
