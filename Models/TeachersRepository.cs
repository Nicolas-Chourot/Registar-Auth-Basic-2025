using JSON_DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JsonDemo.Models
{
    public class TeachersRepository : Repository<Teacher>
    {
        public bool Update(Teacher teacher, List<int> selectedCoursesId)
        {
            BeginTransaction();
            var result = base.Update(teacher);
            if (result) teacher.UpdateAllocations(selectedCoursesId);
            EndTransaction();
            return result;
        }
        public override bool Delete(int Id)
        {
            Teacher teacher = DB.Teachers.Get(Id);
            if (teacher != null)
            {
                BeginTransaction();
                teacher.DeleteAllAllocations();
                var result = base.Delete(Id);
                EndTransaction();
                return result;
            }
            return false;
        }
    }
}