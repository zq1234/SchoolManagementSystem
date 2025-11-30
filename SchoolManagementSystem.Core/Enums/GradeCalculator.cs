namespace SchoolManagementSystem.Core.Enums
{
    public static class GradeCalculator
    {
        public static GradeLetter CalculateGrade(decimal percentage)
        {
            return percentage switch
            {
                >= 97 => GradeLetter.APlus,
                >= 93 => GradeLetter.A,
                >= 90 => GradeLetter.AMinus,
                >= 87 => GradeLetter.BPlus,
                >= 83 => GradeLetter.B,
                >= 80 => GradeLetter.BMinus,
                >= 77 => GradeLetter.CPlus,
                >= 73 => GradeLetter.C,
                >= 70 => GradeLetter.CMinus,
                >= 67 => GradeLetter.DPlus,
                >= 63 => GradeLetter.D,
                >= 60 => GradeLetter.DMinus,
                _ => GradeLetter.F
            };
        }

        public static decimal GetGradePoints(GradeLetter grade)
        {
            return grade switch
            {
                GradeLetter.APlus => 4.0m,
                GradeLetter.A => 4.0m,
                GradeLetter.AMinus => 3.7m,
                GradeLetter.BPlus => 3.3m,
                GradeLetter.B => 3.0m,
                GradeLetter.BMinus => 2.7m,
                GradeLetter.CPlus => 2.3m,
                GradeLetter.C => 2.0m,
                GradeLetter.CMinus => 1.7m,
                GradeLetter.DPlus => 1.3m,
                GradeLetter.D => 1.0m,
                GradeLetter.DMinus => 0.7m,
                GradeLetter.F => 0.0m,
                _ => 0.0m
            };
        }
    }
}