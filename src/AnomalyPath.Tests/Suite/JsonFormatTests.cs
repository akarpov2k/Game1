#nullable enable
using System;
using System.IO;
using AnomalyPath.Core.Data;
using AnomalyPath.Core.Json;

namespace AnomalyPath.Tests
{
    public sealed class JsonFormatTests : TestBase
    {



        public override void Run()
        {
            var deep = new string('[', 20) + "1" + new string(']', 20);
            var valid = new (string Name, string Json, Action<JsonValue> Check)[]
            {
                ("null", "null", v => Equal(JsonKind.Null, v.Kind, "kind")),
                ("true", "true", v => True(v.Bool, "bool")),
                ("false", "false", v => False(v.Bool, "bool")),
                ("целое число", "42", v => Equal(42.0, v.Number, "number")),
                ("дробное отрицательное", "-0.5", v => Equal(-0.5, v.Number, "number")),
                ("экспонента", "1.5e-2", v => Equal(0.015, v.Number, "number")),
                ("пустая строка", "\"\"", v => Equal(string.Empty, v.Text, "text")),
                ("кириллица", "\"Привет\"", v => Equal("Привет", v.Text, "text")),
                (
                    "экранирование",
                    "\"a\\\"b\\\\c\\/d\\be\\ff\\ng\\rh\\ti\"",
                    v => Equal("a\"b\\c/d\be\ff\ng\rh\ti", v.Text, "text")),
                ("unicode escape", "\"\\u0416\\u0443\\u043a\"", v => Equal("Жук", v.Text, "text")),
                ("пустой объект", "{}", v => Equal(0, v.Members.Count, "members")),
                (
                    "объект",
                    "{\"a\":1,\"b\":\"x\",\"c\":true,\"d\":null}",
                    v => Equal(4, v.Members.Count, "members")),
                ("вложенный объект", "{\"a\":{\"b\":{\"c\":true}}}", v => True(v.Get("a").Get("b").Get("c").Bool, "nested")),
                ("пустой массив", "[]", v => Equal(0, v.Items.Count, "items")),
                (
                    "массив смешанных значений",
                    "[1,\"x\",true,null]",
                    v => Equal(4, v.Items.Count, "items")),
                ("массив объектов", "[{\"a\":1},{\"b\":2}]", v => Equal(2, v.Items.Count, "items")),
                (
                    "пробелы вокруг значений",
                    "  { \"a\" : [ 1 , 2 ] }  ",
                    v => Equal(2, v.Get("a").Items.Count, "array")),
                ("переводы строк CRLF", "{\r\n \"a\": 1\r\n}", v => Equal(1.0, v.Get("a").Number, "number")),
                ("табуляция", "{\t\"a\":\t1\t}", v => Equal(1.0, v.Get("a").Number, "number")),
                ("BOM в начале", "\uFEFF{\"a\":1}", v => Equal(1.0, v.Get("a").Number, "number")),
                ("дубли ключей: побеждает последний", "{\"a\":1,\"a\":2}", v => Equal(2.0, v.Get("a").Number, "number")),
                ("глубокая вложенность", deep, v => True(v.Items[0].IsArray, "nested array")),
                ("числа в массиве", "[0,-1,2.5,1e3]", v => Equal(4, v.Items.Count, "items"))
            };

            foreach (var testCase in valid)
            {
                Check($"json: {testCase.Name}", () =>
                {
                    var value = JsonParser.Parse(testCase.Json);
                    testCase.Check(value);
                });
            }

            var invalid = new (string Name, string Json)[]
            {
                ("пустой документ", string.Empty),
                ("только пробелы", "   "),
                ("обрыв строки", "\"abc"),
                ("запятая перед закрытием объекта", "{\"a\":1,}"),
                ("запятая перед закрытием массива", "[1,]"),
                ("одинарные кавычки", "{'a':1}"),
                ("ключ без кавычек", "{a:1}"),
                ("нет двоеточия", "{\"a\" 1}"),
                ("нет значения", "{\"a\":}"),
                ("нет закрывающей скобки", "{\"a\":1"),
                ("нет закрывающей скобки массива", "[1,2"),
                ("лишний текст после значения", "{} x"),
                ("плохой литерал", "tru"),
                ("неизвестный escape", "\"\\q\""),
                ("обрезанный unicode", "\"\\u12\""),
                ("NaN", "NaN"),
                ("комментарий", "{/*x*/\"a\":1}"),
                ("нет ключа", "{:1}"),
                ("два значения подряд", "1 2")
            };

            foreach (var testCase in invalid)
            {
                Check($"json-ошибка: {testCase.Name}", () =>
                {
                    var error = Throws<DataException>(() => JsonParser.Parse(testCase.Json, "fixture.json"), testCase.Name);
                    Equal("fixture.json", error.Path, "path");
                });
            }
        }
    }
}
