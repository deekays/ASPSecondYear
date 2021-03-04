using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

namespace SDClassLibrary
{
    public class SDEmailAnnotation : ValidationAttribute
    {
        public SDEmailAnnotation(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }
            else
            {
                try
                {
                    MailAddress mail = new MailAddress(value.ToString());
                }
                catch (FormatException)
                {
                    return new ValidationResult(ErrorMessage);
                }
            }
            return ValidationResult.Success;
        }
    }
}
