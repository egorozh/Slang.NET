using Slang.Generator.Domain.Entities;
using Slang.Generator.Domain.Nodes.Nodes;
using static Slang.Tests.Helpers.TextNodeBuilder;

namespace Slang.Tests.Unit.Model;

public class NodeTests
{
    public class ListNodeTests
    {
        [Test]
        public void PlainStrings()
        {
            var node = new ListNode(
                Path: "",
                Comment: null,
                Modifiers: new Dictionary<string, string>(),
                Entries:
                [
                    TextNode("Hello"),
                    TextNode("Hi")
                ]
            );

            Assert.That(node.GenericType, Is.EqualTo("string"));
        }

        [Test]
        public void ParameterizedStrings()
        {
            var node = new ListNode(
                Path: "",
                Comment: null,
                Modifiers: new Dictionary<string, string>(),
                Entries:
                [
                    TextNode("Hello"),
                    TextNode("Hi {name}")
                ]
            );

            Assert.That(node.GenericType, Is.EqualTo("dynamic"));
        }

        [Test]
        public void NestedList()
        {
            var node = new ListNode(
                Path: "",
                Comment: null,
                Modifiers: new Dictionary<string, string>(),
                Entries:
                [
                    new ListNode(
                        Path: "",
                        Comment: null,
                        Modifiers: new Dictionary<string, string>(),
                        Entries:
                        [
                            TextNode("Hello"),
                            TextNode("Hi {name}")
                        ]
                    ),
                    new ListNode(
                        Path: "",
                        Comment: null,
                        Modifiers: new Dictionary<string, string>(),
                        Entries:
                        [
                            TextNode("Hello"),
                            TextNode("Hi {name}")
                        ]
                    )
                ]
            );

            Assert.That(node.GenericType, Is.EqualTo("List<dynamic>"));
        }

        [Test]
        public void DeeperNestedList()
        {
            var node = new ListNode(
                Path: "",
                Comment: null,
                Modifiers: new Dictionary<string, string>(),
                Entries:
                [
                    new ListNode(
                        Path: "",
                        Comment: null,
                        Modifiers: new Dictionary<string, string>(),
                        Entries:
                        [
                            new ListNode(
                                Path: "",
                                Comment: null,
                                Modifiers: new Dictionary<string, string>(),
                                Entries:
                                [
                                    TextNode("Hello"),
                                    TextNode("Hi")
                                ]
                            )
                        ]
                    ),
                    new ListNode(
                        Path: "",
                        Comment: null,
                        Modifiers: new Dictionary<string, string>(),
                        Entries:
                        [
                            new ListNode(
                                Path: "",
                                Comment: null,
                                Modifiers: new Dictionary<string, string>(),
                                Entries:
                                [
                                    TextNode("Hello"),
                                    TextNode("Hi")
                                ]
                            ),

                            new ListNode(
                                Path: "",
                                Comment: null,
                                Modifiers: new Dictionary<string, string>(),
                                Entries:
                                [
                                    TextNode("Hello"),
                                    TextNode("Hi")
                                ]
                            )
                        ]
                    )
                ]);

            Assert.That(node.GenericType, Is.EqualTo("List<List<string>>"));
        }

        [Test]
        public void Class()
        {
            var node = new ListNode(
                Path: "",
                Comment: null,
                Modifiers: new Dictionary<string, string>(),
                Entries:
                [
                    new ObjectNode(
                        Path: "",
                        Comment: null,
                        Modifiers: new Dictionary<string, string>(),
                        Entries: new Dictionary<string, Node>
                        {
                            {"key0", TextNode("Hi")}
                        },
                        IsMap: false
                    ),
                    new ObjectNode(
                        Path: "",
                        Comment: null,
                        Modifiers: new Dictionary<string, string>(),
                        Entries: new Dictionary<string, Node>
                        {
                            {"key0", TextNode("Hi")}
                        },
                        IsMap: false
                    )
                ]);

            Assert.That(node.GenericType, Is.EqualTo("dynamic"));
        }

        [Test]
        public void Map()
        {
            var node = new ListNode(
                Path: "",
                Comment: null,
                Modifiers: new Dictionary<string, string>(),
                Entries:
                [
                    new ObjectNode(
                        Path: "",
                        Comment: null,
                        Modifiers: new Dictionary<string, string>(),
                        Entries: new Dictionary<string, Node>
                        {
                            {"key0", TextNode("Hi")}
                        },
                        IsMap: true
                    ),
                    new ObjectNode(
                        Path: "",
                        Comment: null,
                        Modifiers: new Dictionary<string, string>(),
                        Entries: new Dictionary<string, Node>
                        {
                            {"key0", TextNode("Hi")}
                        },
                        IsMap: true
                    )
                ]);

            Assert.That(node.GenericType, Is.EqualTo("Dictionary<string, string>"));
        }
    }

    public class StringTextNodeTests
    {
        [Test]
        public void NoArguments()
        {
            const string test = "No arguments";
            var node = TextNode(test);
            Assert.Multiple(() =>
            {
                Assert.That(test, Is.EqualTo(node.Content));
                Assert.That(new HashSet<string>(), Is.EqualTo(node.Params));
            });
        }

        [Test]
        public void OneArgument()
        {
            const string test = "I have one argument named {apple}.";
            var node = TextNode(test);
            Assert.Multiple(() =>
            {
                Assert.That(node.Content, Is.EqualTo("I have one argument named {apple}."));
                Assert.That(new HashSet<string> {"apple"}, Is.EqualTo(node.Params));
            });
        }

        [Test]
        public void OneArgumentWithoutSpace()
        {
            const string test = "I have one argument named{apple}.";
            var node = TextNode(test);
            Assert.Multiple(() =>
            {
                Assert.That(node.Content, Is.EqualTo("I have one argument named{apple}."));
                Assert.That(new HashSet<string> {"apple"}, Is.EqualTo(node.Params));
            });
        }

        [Test]
        public void OneArgumentFollowedByUnderscore()
        {
            const string test = "This string has one argument named {tom}_";
            var node = TextNode(test);
            Assert.Multiple(() =>
            {
                Assert.That(node.Content, Is.EqualTo("This string has one argument named {tom}_"));
                Assert.That(new HashSet<string> {"tom"}, Is.EqualTo(node.Params));
            });
        }

        [Test]
        public void OneArgumentFollowedByNumber()
        {
            const string test = "This string has one argument named {tom}7";
            var node = TextNode(test);
            Assert.Multiple(() =>
            {
                Assert.That(node.Content, Is.EqualTo("This string has one argument named {tom}7"));
                Assert.That(new HashSet<string> {"tom"}, Is.EqualTo(node.Params));
            });
        }

        [Test]
        public void OneArgumentWithFakeArguments()
        {
            const string test = "$ I have one argument named {apple} but this is $fake. \\$ $";
            var node = TextNode(test);
            Assert.Multiple(() =>
            {
                Assert.That(node.Content,
                    Is.EqualTo("$ I have one argument named {apple} but this is $fake. \\$ $"));
                Assert.That(new HashSet<string> {"apple"}, Is.EqualTo(node.Params));
            });
        }

        [Test]
        public void OneEscapedArgument()
        {
            const string test = "I have one argument named \\{apple}.";
            var node = TextNode(test);
            Assert.Multiple(() =>
            {
                Assert.That(node.Content, Is.EqualTo("I have one argument named {apple}."));
                Assert.That(new HashSet<string>(), Is.EqualTo(node.Params));
            });
        }

        [Test]
        public void OneArgumentWithLink()
        {
            const string test = "{apple} is linked to @:wow!";
            var node = TextNode(test);
            Assert.Multiple(() =>
            {
                Assert.That(node.Content, Is.EqualTo("{apple} is linked to {_root.Wow}!"));
                Assert.That(new HashSet<string> {"apple"}, Is.EqualTo(node.Params));
            });
        }

        [Test]
        public void WithCase()
        {
            const string test = "Nice {cool_hi} {wow} {yes}a {no_yes}";
            var node = TextNode(test, CaseStyle.Camel);
            Assert.Multiple(() =>
            {
                Assert.That(node.Content, Is.EqualTo("Nice {coolHi} {wow} {yes}a {noYes}"));
                Assert.That(new HashSet<string> {"coolHi", "wow", "yes", "noYes"}, Is.EqualTo(node.Params));
            });
        }

        [Test]
        public void WithLinks()
        {
            const string test = "{myArg} @:myLink";
            var node = TextNode(test);
            Assert.Multiple(() =>
            {
                Assert.That(node.Content, Is.EqualTo("{myArg} {_root.MyLink}"));
                Assert.That(new HashSet<string> {"myArg"}, Is.EqualTo(node.Params));
            });
        }

        [Test]
        public void WithEscapedLinks()
        {
            const string test = "{myArg} @:linkA @:{linkB}hello @:{linkC}";
            var node = TextNode(test);
            Assert.Multiple(() =>
            {
                Assert.That(node.Content, Is.EqualTo("{myArg} {_root.LinkA} {_root.LinkB}hello {_root.LinkC}"));
                Assert.That(new HashSet<string> {"myArg"}, Is.EqualTo(node.Params));
            });
        }
    }
}