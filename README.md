# Snork.JsonCommentStripper

> Strip comments from JSON. Lets you use comments in your JSON files!

This is now possible:

```js
{
	// Rainbows
	"unicorn": /*  ❤ */ "cake"
}
```

It will replace single-line comments `//` and multi-line comments `/**/` with whitespace. This allows JSON error positions to remain as close as possible to the original source.

## Usage

```js
import stripJsonComments from 'strip-json-comments';

string json = @"{
	// Rainbows
	\"unicorn\": /*  ❤ */ \"cake\"
}";

Console.WriteLine(Stripper.Execute(json));
//=> {unicorn: 'cake'}
```

## API

### Stripper.StripJsonComments(jsonString, options?)

#### input

Type: `string`

Accepts a string with JSON and returns a string without comments.

#### options

Type: StripperOptions

##### TrailingCommas

Type: `boolean`\
Default: `false`

Strip trailing commas in addition to comments.

##### ReplaceWithWhiteSpace

Type: `boolean`\
Default: `true`

Replace comments and trailing commas with whitespace instead of stripping them entirely.

## Related

This code is adapted from a Javascript version by [Sindre Sorhus](https://github.com/sindresorhus). 
