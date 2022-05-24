///////////////////////////////////////////////////////////////////////////////
// SAMPLE: Generates random password, which complies with the strong password
//         rules and does not contain ambiguous characters.
//
// To run this sample, create a new Visual C# project using the Console
// Application template and replace the contents of the Class1.cs file with
// the code below.
//
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
//
// Copyright (C) 2004 Obviex(TM). All rights reserved.
///////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Nameless.Security {

    /// <summary>
    /// This class can generate random passwords, which do not include ambiguous
    /// characters, such as I, l, and 1. The generated password will be made of
    /// 7-bit ASCII symbols. Every four characters will include one lower case
    /// character, one upper case character, one number, and one special symbol
    /// (such as '%') in a random order. The password will always start with an
    /// alpha-numeric character; it will not start with a special symbol (we do
    /// this because some back-end systems do not like certain special
    /// characters in the first position).
    /// </summary>
    public sealed class RandomPasswordGenerator : IPasswordGenerator {

        #region Private Constants

        // Define supported password characters divided into groups.
        // You can add (or remove) characters to (from) these groups.
        private const string PASSWORD_CHARS_LCASE = "abcdefgijkmnopqrstwxyz";
        private const string PASSWORD_CHARS_UCASE = "ABCDEFGHJKLMNPQRSTWXYZ";
        private const string PASSWORD_CHARS_NUMERIC = "0123456789";
        private const string PASSWORD_CHARS_SPECIAL = "*$-+?_&=!%{}/";

        #endregion

        #region IPasswordGenerator Members

        public string? Generate(PasswordGeneratorOptions? opts = null) {
            var options = opts ?? new();

            // Make sure that input parameters are valid.
            var minLength = options.MinLength;
            var maxLength = options.MaxLength;

            if (minLength <= 0 || maxLength <= 0 || minLength > maxLength) {
                return null;
            }

            // Create a local array containing supported password characters
            // grouped by types. You can remove character groups from this
            // array, but doing so will weaken the password strength.
            var charList = new List<char[]>();
            if (options.UseLowerCase) { charList.Add(PASSWORD_CHARS_LCASE.ToCharArray()); }
            if (options.UseUpperCase) { charList.Add(PASSWORD_CHARS_UCASE.ToCharArray()); }
            if (options.UseNumeric) { charList.Add(PASSWORD_CHARS_NUMERIC.ToCharArray()); }
            if (options.UseSpecialChars) { charList.Add(PASSWORD_CHARS_SPECIAL.ToCharArray()); }
            if (charList.Count == 0) {
                charList.Add(PASSWORD_CHARS_UCASE.ToCharArray());
                charList.Add(PASSWORD_CHARS_NUMERIC.ToCharArray());
            }
            var charGroups = charList.ToArray();

            // Use this array to track the number of unused characters in each
            // character group.
            var charsLeftInGroup = new int[charGroups.Length];

            // Initially, all characters in each group are not used.
            for (var idx = 0; idx < charsLeftInGroup.Length; idx++) {
                charsLeftInGroup[idx] = charGroups[idx].Length;
            }

            // Use this array to track (iterate through) unused character groups.
            var leftGroupsOrder = new int[charGroups.Length];

            // Initially, all character groups are not used.
            for (var idx = 0; idx < leftGroupsOrder.Length; idx++) {
                leftGroupsOrder[idx] = idx;
            }

            // Because we cannot use the default randomizer, which is based on the
            // current time (it will produce the same "random" number within a
            // second), we will use a random number generator to seed the
            // randomizer.

            // Use a 4-byte array to fill it with random bytes and convert it then
            // to an integer value.
            var randomBytes = new byte[4];

            // Generate 4 random bytes.
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);

            // Convert 4 bytes into a 32-bit integer value.
            var seed = (randomBytes[0] & 0x7f) << 24 |
                       randomBytes[1] << 16 |
                       randomBytes[2] << 8 |
                       randomBytes[3];

            // Now, this is real randomization.
            var random = new Random(seed);

            // This array will hold password characters.
            // Allocate appropriate memory for the password.
            var password = minLength < maxLength
                ? new char[random.Next(minLength, maxLength + 1)]
                : new char[minLength];

            // Index of the last non-processed group.
            var lastLeftGroupsOrderIdx = leftGroupsOrder.Length - 1;

            // Generate password characters one at a time.
            for (var idx = 0; idx < password.Length; idx++) {
                // Index which will be used to track not processed character groups.
                // If only one character group remained unprocessed, process it;
                // otherwise, pick a random character group from the unprocessed
                // group list. To allow a special character to appear in the
                // first position, increment the second parameter of the Next
                // function call by one, i.e. lastLeftGroupsOrderIdx + 1.
                var nextLeftGroupsOrderIdx = lastLeftGroupsOrderIdx != 0
                    ? random.Next(0, lastLeftGroupsOrderIdx)
                    : 0;

                // Index of the next character group to be processed.
                // Get the actual index of the character group, from which we will
                // pick the next character.
                var nextGroupIdx = leftGroupsOrder[nextLeftGroupsOrderIdx];

                // Index of the last non-processed character in a group.
                // Get the index of the last unprocessed characters in this group.
                var lastCharIdx = charsLeftInGroup[nextGroupIdx] - 1;

                // Index of the next character to be added to password.
                // If only one unprocessed character is left, pick it; otherwise,
                // get a random character from the unused character list.
                var nextCharIdx = lastCharIdx != 0
                    ? random.Next(0, lastCharIdx + 1)
                    : 0;

                // Add this character to the password.
                password[idx] = charGroups[nextGroupIdx][nextCharIdx];

                // There are more unprocessed characters left.
                if (lastCharIdx != 0) {
                    // Swap processed character with the last unprocessed character
                    // so that we don't pick it until we process all characters in
                    // this group.
                    if (lastCharIdx != nextCharIdx) {
                        var temp = charGroups[nextGroupIdx][lastCharIdx];

                        charGroups[nextGroupIdx][lastCharIdx] = charGroups[nextGroupIdx][nextCharIdx];
                        charGroups[nextGroupIdx][nextCharIdx] = temp;
                    }
                    // Decrement the number of unprocessed characters in
                    // this group.
                    charsLeftInGroup[nextGroupIdx]--;
                } else {
                    // If we processed the last character in this group, start over.
                    charsLeftInGroup[nextGroupIdx] = charGroups[nextGroupIdx].Length;
                }

                // There are more unprocessed groups left.
                if (lastLeftGroupsOrderIdx != 0) {
                    // Swap processed group with the last unprocessed group
                    // so that we don't pick it until we process all groups.
                    if (lastLeftGroupsOrderIdx != nextLeftGroupsOrderIdx) {
                        var temp = leftGroupsOrder[lastLeftGroupsOrderIdx];

                        leftGroupsOrder[lastLeftGroupsOrderIdx] = leftGroupsOrder[nextLeftGroupsOrderIdx];
                        leftGroupsOrder[nextLeftGroupsOrderIdx] = temp;
                    }
                    // Decrement the number of unprocessed groups.
                    lastLeftGroupsOrderIdx--;
                } else {
                    // If we processed the last group, start all over.
                    lastLeftGroupsOrderIdx = leftGroupsOrder.Length - 1;
                }
            }

            // Convert password characters into a string and return the result.
            return new string(password);
        }

        #endregion
    }
}
