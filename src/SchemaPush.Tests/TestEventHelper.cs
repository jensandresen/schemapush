using System;
using SchemaPush.App;
using Xunit;

namespace SchemaPush.Tests
{
    public class TestEventHelper
    {
        [Fact]
        public void returns_expected_when_parsing_empty_string()
        {
            var result = EventHelper.Parse(Array.Empty<string>());
            Assert.Empty(result);
        }
        
        [Fact]
        public void returns_expected_when_parsing_content_with_no_elements()
        {
            var result = EventHelper.Parse(new[]
            {
                "foo", "bar"
            });
            
            Assert.Empty(result);
        }

        [Fact]
        public void parse_single_returns_expected_begin()
        {
            var result = EventHelper.ParseSingle(new[]
            {
                "BEGIN:VEVENT",
                "DTSTART:20000101T000000",
                "DTEND:20000101T000000",
                "END:VEVENT",
            });

            Assert.Equal(DateTime.Parse("2000-01-01 00:00"), result?.Begin);
        }

        [Fact]
        public void parse_single_returns_expected_end()
        {
            var result = EventHelper.ParseSingle(new[]
            {
                "BEGIN:VEVENT",
                "DTSTART:20000101T000000",
                "DTEND:20000101T000000",
                "END:VEVENT",
            });

            Assert.Equal(DateTime.Parse("2000-01-01 00:00"), result?.End);
        }

        [Fact]
        public void parse_single_returns_expected_description()
        {
            var result = EventHelper.ParseSingle(new[]
            {
                "BEGIN:VEVENT",
                "DTSTART:20000101T000000",
                "DTEND:20000101T000000",
                "DESCRIPTION:foo",
                "END:VEVENT",
            });

            Assert.Equal("foo", result?.Description);
        }

        [Fact]
        public void parse_single_returns_expected_summary()
        {
            var result = EventHelper.ParseSingle(new[]
            {
                "BEGIN:VEVENT",
                "SUMMARY:foo",
                "DTSTART:20000101T000000",
                "DTEND:20000101T000000",
                "END:VEVENT",
            });

            Assert.Equal("foo", result?.Summary);
        }

        [Fact]
        public void parse_single_returns_expected_location()
        {
            var result = EventHelper.ParseSingle(new[]
            {
                "BEGIN:VEVENT",
                "LOCATION:foo",
                "DTSTART:20000101T000000",
                "DTEND:20000101T000000",
                "END:VEVENT",
            });

            Assert.Equal("foo", result?.Location);
        }
        
        [Fact]
        public void returns_expected_vevent_when_parsing_content_with_single_element()
        {
            var result = EventHelper.Parse(new[]
            {
                "BEGIN:VEVENT",
                "DTSTART:20000101T000000",
                "DTEND:20000101T000000",
                "SUMMARY:foo",
                "DESCRIPTION:foo",
                "LOCATION:foo",
                "END:VEVENT",
            });

            var vEvent = Assert.Single(result);
            
            Assert.Equal(DateTime.Parse("2000-01-01 00:00"), vEvent.Begin);
            Assert.Equal(DateTime.Parse("2000-01-01 00:00"), vEvent.End);
            Assert.Equal("foo", vEvent.Summary);
            Assert.Equal("foo", vEvent.Description);
            Assert.Equal("foo", vEvent.Location);
        }

        [Fact]
        public void returns_expected_vevent_when_parsing_content_with_multiple_elements()
        {
            var result = EventHelper.Parse(new[]
            {
                "BEGIN:VEVENT",
                "DTSTART:20000101T000000",
                "DTEND:20000101T000000",
                "SUMMARY:foo",
                "DESCRIPTION:foo",
                "LOCATION:foo",
                "END:VEVENT",
                "BEGIN:VEVENT",
                "DTSTART:20000101T000000",
                "DTEND:20000101T000000",
                "SUMMARY:bar",
                "DESCRIPTION:bar",
                "LOCATION:bar",
                "END:VEVENT",
            });

            var expected = new[]
            {
                new Event(begin: DateTime.Parse("2000-01-01 00:00"),
                    end: DateTime.Parse("2000-01-01 00:00"),
                    location: "foo",
                    summary: "foo",
                    description: "foo"),
                new Event(begin: DateTime.Parse("2000-01-01 00:00"),
                    end: DateTime.Parse("2000-01-01 00:00"),
                    location: "bar",
                    summary: "bar",
                    description: "bar"),
            };
            
            Assert.Equal(expected, result);
        }

        [Fact]
        public void returns_expected_when_parsing_content_with_element_missing_end_tag()
        {
            var result = EventHelper.Parse(new[]
            {
                "BEGIN:VEVENT",
                "DTSTART:20000101T000000",
                "DTEND:20000101T000000",
                "SUMMARY:foo",
                "DESCRIPTION:foo",
                "LOCATION:foo",
            });

            Assert.Empty(result);
        }

        [Fact]
        public void returns_expected_when_parsing_content_with_element_missing_begin_tag()
        {
            var result = EventHelper.Parse(new[]
            {
                "DTSTART:20000101T000000",
                "DTEND:20000101T000000",
                "SUMMARY:foo",
                "DESCRIPTION:foo",
                "LOCATION:foo",
                "END:VEVENT",
            });

            Assert.Empty(result);
        }

        [Fact]
        public void empty_lines_before_an_event_is_ignored()
        {
            var result = EventHelper.Parse(new[]
            {
                "",
                " ",
                "  ",
                "BEGIN:VEVENT",
                "DTSTART:20000101T000000",
                "DTEND:20000101T000000",
                "SUMMARY:foo",
                "DESCRIPTION:foo",
                "LOCATION:foo",
                "END:VEVENT",
            });

            var expected = new Event(
                begin: DateTime.Parse("2000-01-01 00:00"),
                end: DateTime.Parse("2000-01-01 00:00"),
                location: "foo",
                summary: "foo",
                description: "foo"
            );
            
            Assert.Equal(new[] {expected}, result);
        }

        [Fact]
        public void empty_lines_after_an_event_is_ignored()
        {
            var result = EventHelper.Parse(new[]
            {
                "BEGIN:VCALENDAR",
                "BEGIN:VEVENT",
                "DTSTART:20000101T000000",
                "DTEND:20000101T000000",
                "SUMMARY:foo",
                "DESCRIPTION:foo",
                "LOCATION:foo",
                "END:VEVENT",
                "",
                " ",
                "  ",
                "END:VCALENDAR",
            });

            var expected = new Event(
                begin: DateTime.Parse("2000-01-01 00:00"),
                end: DateTime.Parse("2000-01-01 00:00"),
                location: "foo",
                summary: "foo",
                description: "foo"
            );

            Assert.Equal(new[] {expected}, result);
        }

        [Fact]
        public void empty_lines_inside_an_event_is_ignored()
        {
            var result = EventHelper.Parse(new[]
            {
                "BEGIN:VEVENT",
                "DTSTART:20000101T000000",
                "DTEND:20000101T000000",
                "",
                " ",
                "  ",
                "SUMMARY:foo",
                "DESCRIPTION:foo",
                "LOCATION:foo",
                "END:VEVENT",
            });

            var expected = new Event(
                begin: DateTime.Parse("2000-01-01 00:00"),
                end: DateTime.Parse("2000-01-01 00:00"),
                location: "foo",
                summary: "foo",
                description: "foo"
            );

            Assert.Equal(new[] {expected}, result);
        }

        [Fact]
        public void duno_duno()
        {
            var result = EventHelper.Parse(new[]
            {
                "BEGIN:VCALENDAR",
                "VERSION:2.0",
                "PRODID:icalendar-ruby",
                "CALSCALE:GREGORIAN",
                "METHOD:PUBLISH",
                "X-WR-CALNAME:docendo",
                "BEGIN:VEVENT",
                "DTSTAMP:20210827T125212Z",
                "UID:00640e5d-1478-4f39-b7d2-e074a0b0467e",
                "DTSTART:20220609T073000Z",
                "DTEND:20220609T075000Z",
                "SUMMARY:Frikvarter, 21-22_Tilsyn Jonstrup",
                "DESCRIPTION:",
                "LOCATION:",
                "END:VEVENT",
                "END:VCALENDAR",
            });

            var expected = new Event(
                begin: DateTime.Parse("2000-01-01 00:00"),
                end: DateTime.Parse("2000-01-01 00:00"),
                location: "foo",
                summary: "foo",
                description: "foo"
            );

            // Assert.Equal(new[] { expected }, result);
            Assert.NotEmpty(result);
        }
    }
}