using IReckonUpload.Business.ModelConverter.Core;
using IReckonUpload.Business.ModelConverter.Middlewares;
using System;

namespace IReckonUpload.Business.ModelConverter
{
    public static class FileToModelConverterExtensions
    {
        public static IFileToModelConverter Use<T>(this IFileToModelConverter converter) where T: IFileToModelConverterBaseMiddleware
        {
            converter.UseMiddleware<T>();

            return converter;
        }

        public static IFileToModelConverter Use<T>(this IFileToModelConverter converter, Action<T> configure) where T : IFileToModelConverterBaseMiddleware
        {
            converter.UseMiddleware<T>(configure);

            return converter;
        }
    }
}
