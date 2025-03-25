﻿using JSON_DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;

namespace JsonDemo.Models
{
    public class UsersRepository : Repository<User>
    {

        #region Password Encryption
        const int SaltSize = 20;
        private static string CreateSalt(int size)
        {
            RNGCryptoServiceProvider randomNumberGenerator = new RNGCryptoServiceProvider();
            byte[] buff = new byte[size];
            randomNumberGenerator.GetBytes(buff);
            return Convert.ToBase64String(buff); // web compatible format
        }
        private static string HashPassword(string password, string salt = "")
        {
            if (string.IsNullOrEmpty(salt)) salt = CreateSalt(SaltSize);
            string saltedPassword = password + salt;
            HashAlgorithm encryptAlgorithm = new SHA256CryptoServiceProvider();
            byte[] bytValue = System.Text.Encoding.UTF8.GetBytes(saltedPassword);
            byte[] bytHash = encryptAlgorithm.ComputeHash(bytValue);
            string base64 = Convert.ToBase64String(bytHash); // web compatible format
            return base64 + salt;
        }
        private static bool VerifyPassword(string password, string storedPassword)
        {
            string salt = storedPassword.Substring(storedPassword.Length - CreateSalt(SaltSize).Length);
            string hashedPassword = HashPassword(password, salt);
            return hashedPassword == storedPassword;
        }
        #endregion
        public bool EmailExist(string email)
        {
            return ToList().Where(u => u.Email.ToLower() == email.ToLower()).FirstOrDefault() != null;
        }
        public User GetUser(LoginCredential loginCredential)
        {
            User user = ToList().Where(u => u.Email.ToLower() == loginCredential.Email.ToLower()).FirstOrDefault();
            if (user != null && VerifyPassword(loginCredential.Password, user.Password))
                return user.Copy();
            return null;
        }
        public void SetOnline(Object userObj, bool online)
        {
            var user = (User)userObj;
            if (user != null)
            {
                user = DB.Users.Get(user.Id);
                user.Online = online;
                Update(user);
            }
        }
        public override int Add(User user)
        {
            user.Password = HashPassword(user.Password);
            return base.Add(user);
        }
        public override bool Update(User user)
        {
            User storedUser = Get(user.Id);
            if (user.Password != storedUser.Password) // new password
                user.Password = HashPassword(user.Password);
            return base.Update(user);
        }
        public override bool Delete(int userId)
        {
            try
            {
                User userToDelete = DB.Users.Get(userId);
                if (userToDelete != null)
                {
                    BeginTransaction();
                    /*
                     * Deletion of data related to user
                     */
                    base.Delete(userId);
                    EndTransaction();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Remove user failed : Message - {ex.Message}");
                EndTransaction();
                return false;
            }
        }
    }
}