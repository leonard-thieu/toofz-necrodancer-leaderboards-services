using System;

namespace toofz.Services
{
    interface IConsole
    {
        /// Obtains the next character or function key pressed by the user. The pressed key
        /// is optionally displayed in the console window.
        /// </summary>
        /// <param name="intercept">
        /// Determines whether to display the pressed key in the console window. true to
        /// not display the pressed key; otherwise, false.
        /// </param>
        /// <returns>
        /// An object that describes the <see cref="ConsoleKey"/> constant and Unicode character,
        /// if any, that correspond to the pressed console key. The <see cref="ConsoleKeyInfo"/>
        /// object also describes, in a bitwise combination of <see cref="ConsoleModifiers"/> values,
        /// whether one or more Shift, Alt, or Ctrl modifier keys was pressed simultaneously
        /// with the console key.
        /// </returns>
        ConsoleKeyInfo ReadKey(bool intercept);
    }
}
