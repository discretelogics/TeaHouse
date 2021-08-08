using System;
using System.IO;

namespace TeaTime
{
    public class Guard
    {
        #region public methods
        /// <summary>
        /// Ensures that an argument is not null. If it is, an ArgumentException will be thrown.
        /// </summary>
        /// <param name="objectToValidate">The arguments value.</param>
        /// <param name="argumentName">The arguments name.</param>
        /// <exception cref="ArgumentNullException"/>
        public static void ArgumentNotNull(object objectToValidate, string argumentName)
        {
            if (objectToValidate == null)
            {
                throw new ArgumentNullException(argumentName);
            }
        }

        /// <summary>
        /// Ensures that an argument is not null or empty. If it is, an ArgumentException will be thrown.
        /// </summary>
        /// <param name="objectToValidate">The arguments value.</param>
        /// <param name="argumentName">The arguments name.</param>
        /// <exception cref="ArgumentException"/>
        public static void ArgumentNotNullOrEmpty(string objectToValidate, string argumentName)
        {
            if (String.IsNullOrEmpty(objectToValidate))
            {
                throw new ArgumentException("The provided string must not be null or empty.", argumentName);
            }
        }

        /// <summary>
        /// Ensures that an argument is not null, empty or a whitespace. If it is, an ArgumentException will be thrown.
        /// </summary>
        /// <param name="objectToValidate">The arguments value.</param>
        /// <param name="argumentName">The arguments name.</param>
        /// <exception cref="ArgumentException"/>
        public static void ArgumentNotNullOrWhiteSpace(string objectToValidate, string argumentName)
        {
            if (String.IsNullOrWhiteSpace(objectToValidate))
            {
                throw new ArgumentException("The provided string must not be null, empty or a whitespace.", argumentName);
            }
        }

        /// <summary>
        /// Ensures that an argument is not an empty Guid. If it is, an ArgumentException will be thrown.
        /// </summary>
        /// <param name="objectToValidate">The arguments value.</param>
        /// <param name="argumentName">The arguments name.</param>
        /// <exception cref="ArgumentException"/>
        public static void ArgumentNotEmpty(Guid objectToValidate, string argumentName)
        {
            if (objectToValidate == Guid.Empty)
            {
                throw new ArgumentException("The provided Guid must not be empty.", argumentName);
            }
        }

        /// <summary>
        /// Ensures that a file exists. If it is, an ArgumentException will be thrown.
        /// </summary>
        /// <param name="objectToValidate">The arguments value.</param>
        /// <param name="argumentName">The arguments name.</param>
        /// <exception cref="ArgumentException"/>
        public static void ArgumentFileExists(string objectToValidate, string argumentName)
        {
            if (!File.Exists(objectToValidate))
            {
                throw new ArgumentException("File does not exist.", argumentName);
            }
        }

        /// <summary>
        /// Ensures that a file exists. If it is, an ArgumentException will be thrown.
        /// </summary>
        /// <param name="objectToValidate">The arguments value.</param>
        /// <param name="argumentName">The arguments name.</param>
        /// <exception cref="ArgumentException"/>
        public static void ArgumentFileNotExists(string objectToValidate, string argumentName)
        {
            if (File.Exists(objectToValidate))
            {
                throw new ArgumentException("File does not exist.", argumentName);
            }
        }

        /// <summary>
        /// Ensures that a directory. If it is, an ArgumentException will be thrown.
        /// </summary>
        /// <param name="objectToValidate">The arguments value.</param>
        /// <param name="argumentName">The arguments name.</param>
        /// <exception cref="ArgumentException"/>
        public static void ArgumentDirectoryExists(string objectToValidate, string argumentName)
        {
            if (!Directory.Exists(objectToValidate))
            {
                throw new ArgumentException("The provided directory has to exist.", argumentName);
            }
        }

        /// <summary>
        /// Ensures that a directory. If it is, an ArgumentException will be thrown.
        /// </summary>
        /// <param name="objectToValidate">The arguments value.</param>
        /// <param name="argumentName">The arguments name.</param>
        /// <exception cref="ArgumentException"/>
        public static void ArgumentDirectoryNotExists(string objectToValidate, string argumentName)
        {
            if (Directory.Exists(objectToValidate))
            {
                throw new ArgumentException("The provided directory must not exist.", argumentName);
            }
        }
        #endregion
    }
}
