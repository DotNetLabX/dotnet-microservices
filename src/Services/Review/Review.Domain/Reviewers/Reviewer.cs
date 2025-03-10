namespace Review.Domain.Reviewers;

public partial class Reviewer : Person
{
		private HashSet<ReviewerSpecialization> _specializations = new();
		public IReadOnlyCollection<ReviewerSpecialization> Specializations => _specializations;

		//public override string TypeDiscriminator => nameof(Reviewer);
}
