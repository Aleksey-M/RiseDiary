using Microsoft.AspNetCore.Identity;
using System;

namespace Pwd
{
    class Program
    {
        static void Main()
        {
            var passwordHasher = new PasswordHasher<string>();
            Console.WriteLine(passwordHasher.HashPassword(null, ""));
            Console.ReadLine();
        }
    }
}
