using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using SDClassLibrary;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;

namespace SDClubs.Models
{
    [ModelMetadataType(typeof(SDNameAddressMetadata))]
    public partial class NameAddress : IValidatableObject
    {
        private ClubsContext _context;



        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            _context = new ClubsContext();  //requires code somewhere else

            //var _context = validationContext.GetService<ClubsContext>(); // requires dependencyinjection

            // all strings that are null become empty and trim all
            if (FirstName == null)
                FirstName = "";
            FirstName = FirstName.Trim();
            if (LastName == null)
                LastName = "";
            LastName = LastName.Trim();
            if (CompanyName == null)
                CompanyName = "";
            CompanyName = CompanyName.Trim();
            if (StreetAddress == null)
                StreetAddress = "";
            StreetAddress = StreetAddress.Trim();
            if (PostalCode == null)
                PostalCode = "";
            PostalCode = PostalCode.Trim();
            if (ProvinceCode == null)
                ProvinceCode = "";
            ProvinceCode = ProvinceCode.Trim();
            if (Email == null)
                Email = "";
            Email = Email.Trim();
            if (Phone == null)
                Phone = "";
            Phone = Phone.Trim();

            // use our prebuilt string manipulators to capitalize and extract digits
            FirstName = SDStringManipulation.SDCapitalize(FirstName);
            LastName = SDStringManipulation.SDCapitalize(LastName);
            CompanyName = SDStringManipulation.SDCapitalize(CompanyName);
            StreetAddress = SDStringManipulation.SDCapitalize(StreetAddress);
            City = SDStringManipulation.SDCapitalize(City);

            Phone = SDStringManipulation.SDExtractDigits(Phone);

            if(FirstName == "" && LastName == "" && CompanyName == "")
            {
                yield return new ValidationResult("You must enter either First Name, Last Name, or Company Name.");
            }

            if(ProvinceCode != "")
            {
                var province = _context.Province.Where(a => a.ProvinceCode.ToLower() == ProvinceCode.ToLower()).FirstOrDefault(); // pull province
                if(province == null)
                {
                    yield return new ValidationResult("The Province Code must be an existing province code within our database. Good luck figuring them out lol",new string[] { nameof(ProvinceCode) });
                }
                else
                {
                    ProvinceCode = ProvinceCode.ToUpper(); // to save nicely
                    
                    var country = _context.Country.Where(a => a.CountryCode == province.CountryCode).FirstOrDefault();
                    if (PostalCode != "")
                    {
                        if (!SDStringManipulation.SDPostalCodeIsValid(PostalCode.ToUpper(), country.PostalPattern)) // runs the postal through the regex for the country's postal pattern
                        {
                            yield return new ValidationResult("The postal code must match the country's format", new string[] { nameof(PostalCode) });
                        }

                        if (country.Name == "Canada")
                        {
                            PostalCode = PostalCode.ToUpper(); // you would be ashamed of how long I was stuck here before realizing the regex was case sensitive (and that we should be storing them as capitals anyway)
                            if (PostalCode != "")
                            {
                                char firstLetter = PostalCode[0];
                                if (!province.FirstPostalLetter.Contains(firstLetter)) // checks against the saved first letters for province postal codes
                                {
                                    yield return new ValidationResult("The postal code must match the province's postal codes.", new string[] { nameof(PostalCode) });
                                }
                            }
                        }
                        if (PostalCode.Length > 6)
                        {
                            PostalCode.Insert(3, " ");
                        }
                    }
                }
            }

            if(Email == "")
            {
                if(StreetAddress == "" || City == "" || PostalCode == "" || ProvinceCode == "")
                {
                    yield return new ValidationResult("You must submit either a valid email or all postal information.");
                }
            }
            
            Regex regPhone = new Regex(@"^[0-9]{10}$"); // upon coming back I realize I basically did this in my string manipulation and just have to check length
                                                        // but hey it tests length too and I am just super done with this :) 
            if(Phone == "")
            {
                yield return new ValidationResult("Please enter a valid phone number.", new string[] { nameof(Phone) });
            }
            else if(!regPhone.IsMatch(Phone))
            {
                yield return new ValidationResult("Please enter a 10 digit phone number.", new string[] { nameof(Phone) });
            }
            else
            {
                Phone = Phone.Insert(3, "-");
                Phone = Phone.Insert(7, "-");
            }

            yield return ValidationResult.Success;
        }
    }

    public class SDNameAddressMetadata
    {
        public int NameAddressId { get; set; }
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; }
        [Display(Name = "Street Address")]
        public string StreetAddress { get; set; }
        public string City { get; set; }
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; }
        [Display(Name = "Province Code")]
        [Required] // I know this wasn't in the spec, but the database breaks if there's no province code
        public string ProvinceCode { get; set; }
        [SDEmailAnnotation("Invalid Email")]
        public string Email { get; set; }
        [Required]
        public string Phone { get; set; }

        public virtual Province ProvinceCodeNavigation { get; set; }
        public virtual Club Club { get; set; }
        public virtual ICollection<Artist> Artist { get; set; }
    }
}
