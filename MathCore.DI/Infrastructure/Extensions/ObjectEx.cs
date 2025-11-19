using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace MathCore.DI.Infrastructure.Extensions;

internal static class ObjectEx
{
    /// <summary>Проверка на пустую ссылку</summary>
    /// <typeparam name="T">Тип проверяемого объекта</typeparam>
    /// <param name="obj">Проверяемое значение</param>
    /// <param name="Message">Сообщение ошибки</param>
    /// <param name="ParameterName">Название параметра</param>
    /// <returns>Значение, точно не являющееся пустой ссылкой</returns>
    /// <exception cref="InvalidOperationException">В случае если переданное значение <paramref name="obj"/> == <c>null</c> и <paramref name="ParameterName"/> == <c>null</c></exception>
    /// <exception cref="ArgumentNullException">В случае если переданное значение <paramref name="obj"/> == <c>null</c> и <paramref name="ParameterName"/> != <c>null</c></exception>
    [return: NotNull]
    [return: NotNullIfNotNull(nameof(obj))]
    public static T NotNull<T>(this T? obj, string? Message = null, [CallerArgumentExpression(nameof(obj))] string? ParameterName = null!)
        where T : class =>
        obj ?? throw (ParameterName is null
            ? new InvalidOperationException(Message ?? "Пустая ссылка на объект")
            : new ArgumentNullException(ParameterName, Message ?? "Пустая ссылка в значении параметра"));
}
