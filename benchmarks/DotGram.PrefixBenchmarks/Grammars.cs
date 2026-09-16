using System;

using DotGram;

namespace DotGram.PrefixBenchmarks;

[Gram("""
	Start : @int = "GET" => @(0)
		| "POST" => @(1)
		| "PUT" => @(2)
		| "PATCH" => @(3)
		| "DELETE" => @(4)
		| "HEAD" => @(5)
		| "OPTIONS" => @(6)
		| "TRACE" => @(7)
	parse Start
	""", Direct = false, Portable = false, PrefixTables = false)]
static partial class Methods8Default;

[Gram("""
	Start : @int = "GET" => @(0)
		| "POST" => @(1)
		| "PUT" => @(2)
		| "PATCH" => @(3)
		| "DELETE" => @(4)
		| "HEAD" => @(5)
		| "OPTIONS" => @(6)
		| "TRACE" => @(7)
	parse Start
	""", Direct = false, Portable = false, PrefixTables = true)]
static partial class Methods8Tables;

[Gram("""
	Start : @int = "X00=" & value: ['a'..'z']+ => @(0)
		| "X01=" & value: ['a'..'z']+ => @(1)
		| "X02=" & value: ['a'..'z']+ => @(2)
		| "X03=" & value: ['a'..'z']+ => @(3)
	parse Start
	""", Direct = false, Portable = false, PrefixTables = false)]
static partial class Tags4Default;

[Gram("""
	Start : @int = "X00=" & value: ['a'..'z']+ => @(0)
		| "X01=" & value: ['a'..'z']+ => @(1)
		| "X02=" & value: ['a'..'z']+ => @(2)
		| "X03=" & value: ['a'..'z']+ => @(3)
	parse Start
	""", Direct = false, Portable = false, PrefixTables = true)]
static partial class Tags4Tables;

[Gram("""
	Start : @int = "X00=" & value: ['a'..'z']+ => @(0)
		| "X01=" & value: ['a'..'z']+ => @(1)
		| "X02=" & value: ['a'..'z']+ => @(2)
		| "X03=" & value: ['a'..'z']+ => @(3)
		| "X04=" & value: ['a'..'z']+ => @(4)
		| "X05=" & value: ['a'..'z']+ => @(5)
		| "X06=" & value: ['a'..'z']+ => @(6)
		| "X07=" & value: ['a'..'z']+ => @(7)
	parse Start
	""", Direct = false, Portable = false, PrefixTables = false)]
static partial class Tags8Default;

[Gram("""
	Start : @int = "X00=" & value: ['a'..'z']+ => @(0)
		| "X01=" & value: ['a'..'z']+ => @(1)
		| "X02=" & value: ['a'..'z']+ => @(2)
		| "X03=" & value: ['a'..'z']+ => @(3)
		| "X04=" & value: ['a'..'z']+ => @(4)
		| "X05=" & value: ['a'..'z']+ => @(5)
		| "X06=" & value: ['a'..'z']+ => @(6)
		| "X07=" & value: ['a'..'z']+ => @(7)
	parse Start
	""", Direct = false, Portable = false, PrefixTables = true)]
static partial class Tags8Tables;

[Gram("""
	Start : @int = "X00=" & value: ['a'..'z']+ => @(0)
		| "X01=" & value: ['a'..'z']+ => @(1)
		| "X02=" & value: ['a'..'z']+ => @(2)
		| "X03=" & value: ['a'..'z']+ => @(3)
		| "X04=" & value: ['a'..'z']+ => @(4)
		| "X05=" & value: ['a'..'z']+ => @(5)
		| "X06=" & value: ['a'..'z']+ => @(6)
		| "X07=" & value: ['a'..'z']+ => @(7)
		| "X08=" & value: ['a'..'z']+ => @(8)
		| "X09=" & value: ['a'..'z']+ => @(9)
		| "X10=" & value: ['a'..'z']+ => @(10)
		| "X11=" & value: ['a'..'z']+ => @(11)
		| "X12=" & value: ['a'..'z']+ => @(12)
		| "X13=" & value: ['a'..'z']+ => @(13)
		| "X14=" & value: ['a'..'z']+ => @(14)
		| "X15=" & value: ['a'..'z']+ => @(15)
	parse Start
	""", Direct = false, Portable = false, PrefixTables = false)]
static partial class Tags16Default;

[Gram("""
	Start : @int = "X00=" & value: ['a'..'z']+ => @(0)
		| "X01=" & value: ['a'..'z']+ => @(1)
		| "X02=" & value: ['a'..'z']+ => @(2)
		| "X03=" & value: ['a'..'z']+ => @(3)
		| "X04=" & value: ['a'..'z']+ => @(4)
		| "X05=" & value: ['a'..'z']+ => @(5)
		| "X06=" & value: ['a'..'z']+ => @(6)
		| "X07=" & value: ['a'..'z']+ => @(7)
		| "X08=" & value: ['a'..'z']+ => @(8)
		| "X09=" & value: ['a'..'z']+ => @(9)
		| "X10=" & value: ['a'..'z']+ => @(10)
		| "X11=" & value: ['a'..'z']+ => @(11)
		| "X12=" & value: ['a'..'z']+ => @(12)
		| "X13=" & value: ['a'..'z']+ => @(13)
		| "X14=" & value: ['a'..'z']+ => @(14)
		| "X15=" & value: ['a'..'z']+ => @(15)
	parse Start
	""", Direct = false, Portable = false, PrefixTables = true)]
static partial class Tags16Tables;

[Gram("""
	Start : @int = "X00=" & value: ['a'..'z']+ => @(0)
		| "X01=" & value: ['a'..'z']+ => @(1)
		| "X02=" & value: ['a'..'z']+ => @(2)
		| "X03=" & value: ['a'..'z']+ => @(3)
		| "X04=" & value: ['a'..'z']+ => @(4)
		| "X05=" & value: ['a'..'z']+ => @(5)
		| "X06=" & value: ['a'..'z']+ => @(6)
		| "X07=" & value: ['a'..'z']+ => @(7)
		| "X08=" & value: ['a'..'z']+ => @(8)
		| "X09=" & value: ['a'..'z']+ => @(9)
		| "X10=" & value: ['a'..'z']+ => @(10)
		| "X11=" & value: ['a'..'z']+ => @(11)
		| "X12=" & value: ['a'..'z']+ => @(12)
		| "X13=" & value: ['a'..'z']+ => @(13)
		| "X14=" & value: ['a'..'z']+ => @(14)
		| "X15=" & value: ['a'..'z']+ => @(15)
		| "X16=" & value: ['a'..'z']+ => @(16)
		| "X17=" & value: ['a'..'z']+ => @(17)
		| "X18=" & value: ['a'..'z']+ => @(18)
		| "X19=" & value: ['a'..'z']+ => @(19)
		| "X20=" & value: ['a'..'z']+ => @(20)
		| "X21=" & value: ['a'..'z']+ => @(21)
		| "X22=" & value: ['a'..'z']+ => @(22)
		| "X23=" & value: ['a'..'z']+ => @(23)
		| "X24=" & value: ['a'..'z']+ => @(24)
		| "X25=" & value: ['a'..'z']+ => @(25)
		| "X26=" & value: ['a'..'z']+ => @(26)
		| "X27=" & value: ['a'..'z']+ => @(27)
		| "X28=" & value: ['a'..'z']+ => @(28)
		| "X29=" & value: ['a'..'z']+ => @(29)
		| "X30=" & value: ['a'..'z']+ => @(30)
		| "X31=" & value: ['a'..'z']+ => @(31)
	parse Start
	""", Direct = false, Portable = false, PrefixTables = false)]
static partial class Tags32Default;

[Gram("""
	Start : @int = "X00=" & value: ['a'..'z']+ => @(0)
		| "X01=" & value: ['a'..'z']+ => @(1)
		| "X02=" & value: ['a'..'z']+ => @(2)
		| "X03=" & value: ['a'..'z']+ => @(3)
		| "X04=" & value: ['a'..'z']+ => @(4)
		| "X05=" & value: ['a'..'z']+ => @(5)
		| "X06=" & value: ['a'..'z']+ => @(6)
		| "X07=" & value: ['a'..'z']+ => @(7)
		| "X08=" & value: ['a'..'z']+ => @(8)
		| "X09=" & value: ['a'..'z']+ => @(9)
		| "X10=" & value: ['a'..'z']+ => @(10)
		| "X11=" & value: ['a'..'z']+ => @(11)
		| "X12=" & value: ['a'..'z']+ => @(12)
		| "X13=" & value: ['a'..'z']+ => @(13)
		| "X14=" & value: ['a'..'z']+ => @(14)
		| "X15=" & value: ['a'..'z']+ => @(15)
		| "X16=" & value: ['a'..'z']+ => @(16)
		| "X17=" & value: ['a'..'z']+ => @(17)
		| "X18=" & value: ['a'..'z']+ => @(18)
		| "X19=" & value: ['a'..'z']+ => @(19)
		| "X20=" & value: ['a'..'z']+ => @(20)
		| "X21=" & value: ['a'..'z']+ => @(21)
		| "X22=" & value: ['a'..'z']+ => @(22)
		| "X23=" & value: ['a'..'z']+ => @(23)
		| "X24=" & value: ['a'..'z']+ => @(24)
		| "X25=" & value: ['a'..'z']+ => @(25)
		| "X26=" & value: ['a'..'z']+ => @(26)
		| "X27=" & value: ['a'..'z']+ => @(27)
		| "X28=" & value: ['a'..'z']+ => @(28)
		| "X29=" & value: ['a'..'z']+ => @(29)
		| "X30=" & value: ['a'..'z']+ => @(30)
		| "X31=" & value: ['a'..'z']+ => @(31)
	parse Start
	""", Direct = false, Portable = false, PrefixTables = true)]
static partial class Tags32Tables;

[Gram("""
	Start : @int = "X00=" & value: ['a'..'z']+ => @(0)
		| "X01=" & value: ['a'..'z']+ => @(1)
		| "X02=" & value: ['a'..'z']+ => @(2)
		| "X03=" & value: ['a'..'z']+ => @(3)
		| "X04=" & value: ['a'..'z']+ => @(4)
		| "X05=" & value: ['a'..'z']+ => @(5)
		| "X06=" & value: ['a'..'z']+ => @(6)
		| "X07=" & value: ['a'..'z']+ => @(7)
		| "X08=" & value: ['a'..'z']+ => @(8)
		| "X09=" & value: ['a'..'z']+ => @(9)
		| "X10=" & value: ['a'..'z']+ => @(10)
		| "X11=" & value: ['a'..'z']+ => @(11)
		| "X12=" & value: ['a'..'z']+ => @(12)
		| "X13=" & value: ['a'..'z']+ => @(13)
		| "X14=" & value: ['a'..'z']+ => @(14)
		| "X15=" & value: ['a'..'z']+ => @(15)
		| "X16=" & value: ['a'..'z']+ => @(16)
		| "X17=" & value: ['a'..'z']+ => @(17)
		| "X18=" & value: ['a'..'z']+ => @(18)
		| "X19=" & value: ['a'..'z']+ => @(19)
		| "X20=" & value: ['a'..'z']+ => @(20)
		| "X21=" & value: ['a'..'z']+ => @(21)
		| "X22=" & value: ['a'..'z']+ => @(22)
		| "X23=" & value: ['a'..'z']+ => @(23)
		| "X24=" & value: ['a'..'z']+ => @(24)
		| "X25=" & value: ['a'..'z']+ => @(25)
		| "X26=" & value: ['a'..'z']+ => @(26)
		| "X27=" & value: ['a'..'z']+ => @(27)
		| "X28=" & value: ['a'..'z']+ => @(28)
		| "X29=" & value: ['a'..'z']+ => @(29)
		| "X30=" & value: ['a'..'z']+ => @(30)
		| "X31=" & value: ['a'..'z']+ => @(31)
		| "X32=" & value: ['a'..'z']+ => @(32)
		| "X33=" & value: ['a'..'z']+ => @(33)
		| "X34=" & value: ['a'..'z']+ => @(34)
		| "X35=" & value: ['a'..'z']+ => @(35)
		| "X36=" & value: ['a'..'z']+ => @(36)
		| "X37=" & value: ['a'..'z']+ => @(37)
		| "X38=" & value: ['a'..'z']+ => @(38)
		| "X39=" & value: ['a'..'z']+ => @(39)
		| "X40=" & value: ['a'..'z']+ => @(40)
		| "X41=" & value: ['a'..'z']+ => @(41)
		| "X42=" & value: ['a'..'z']+ => @(42)
		| "X43=" & value: ['a'..'z']+ => @(43)
		| "X44=" & value: ['a'..'z']+ => @(44)
		| "X45=" & value: ['a'..'z']+ => @(45)
		| "X46=" & value: ['a'..'z']+ => @(46)
		| "X47=" & value: ['a'..'z']+ => @(47)
		| "X48=" & value: ['a'..'z']+ => @(48)
		| "X49=" & value: ['a'..'z']+ => @(49)
		| "X50=" & value: ['a'..'z']+ => @(50)
		| "X51=" & value: ['a'..'z']+ => @(51)
		| "X52=" & value: ['a'..'z']+ => @(52)
		| "X53=" & value: ['a'..'z']+ => @(53)
		| "X54=" & value: ['a'..'z']+ => @(54)
		| "X55=" & value: ['a'..'z']+ => @(55)
		| "X56=" & value: ['a'..'z']+ => @(56)
		| "X57=" & value: ['a'..'z']+ => @(57)
		| "X58=" & value: ['a'..'z']+ => @(58)
		| "X59=" & value: ['a'..'z']+ => @(59)
		| "X60=" & value: ['a'..'z']+ => @(60)
		| "X61=" & value: ['a'..'z']+ => @(61)
		| "X62=" & value: ['a'..'z']+ => @(62)
		| "X63=" & value: ['a'..'z']+ => @(63)
	parse Start
	""", Direct = false, Portable = false, PrefixTables = false)]
static partial class Tags64Default;

[Gram("""
	Start : @int = "X00=" & value: ['a'..'z']+ => @(0)
		| "X01=" & value: ['a'..'z']+ => @(1)
		| "X02=" & value: ['a'..'z']+ => @(2)
		| "X03=" & value: ['a'..'z']+ => @(3)
		| "X04=" & value: ['a'..'z']+ => @(4)
		| "X05=" & value: ['a'..'z']+ => @(5)
		| "X06=" & value: ['a'..'z']+ => @(6)
		| "X07=" & value: ['a'..'z']+ => @(7)
		| "X08=" & value: ['a'..'z']+ => @(8)
		| "X09=" & value: ['a'..'z']+ => @(9)
		| "X10=" & value: ['a'..'z']+ => @(10)
		| "X11=" & value: ['a'..'z']+ => @(11)
		| "X12=" & value: ['a'..'z']+ => @(12)
		| "X13=" & value: ['a'..'z']+ => @(13)
		| "X14=" & value: ['a'..'z']+ => @(14)
		| "X15=" & value: ['a'..'z']+ => @(15)
		| "X16=" & value: ['a'..'z']+ => @(16)
		| "X17=" & value: ['a'..'z']+ => @(17)
		| "X18=" & value: ['a'..'z']+ => @(18)
		| "X19=" & value: ['a'..'z']+ => @(19)
		| "X20=" & value: ['a'..'z']+ => @(20)
		| "X21=" & value: ['a'..'z']+ => @(21)
		| "X22=" & value: ['a'..'z']+ => @(22)
		| "X23=" & value: ['a'..'z']+ => @(23)
		| "X24=" & value: ['a'..'z']+ => @(24)
		| "X25=" & value: ['a'..'z']+ => @(25)
		| "X26=" & value: ['a'..'z']+ => @(26)
		| "X27=" & value: ['a'..'z']+ => @(27)
		| "X28=" & value: ['a'..'z']+ => @(28)
		| "X29=" & value: ['a'..'z']+ => @(29)
		| "X30=" & value: ['a'..'z']+ => @(30)
		| "X31=" & value: ['a'..'z']+ => @(31)
		| "X32=" & value: ['a'..'z']+ => @(32)
		| "X33=" & value: ['a'..'z']+ => @(33)
		| "X34=" & value: ['a'..'z']+ => @(34)
		| "X35=" & value: ['a'..'z']+ => @(35)
		| "X36=" & value: ['a'..'z']+ => @(36)
		| "X37=" & value: ['a'..'z']+ => @(37)
		| "X38=" & value: ['a'..'z']+ => @(38)
		| "X39=" & value: ['a'..'z']+ => @(39)
		| "X40=" & value: ['a'..'z']+ => @(40)
		| "X41=" & value: ['a'..'z']+ => @(41)
		| "X42=" & value: ['a'..'z']+ => @(42)
		| "X43=" & value: ['a'..'z']+ => @(43)
		| "X44=" & value: ['a'..'z']+ => @(44)
		| "X45=" & value: ['a'..'z']+ => @(45)
		| "X46=" & value: ['a'..'z']+ => @(46)
		| "X47=" & value: ['a'..'z']+ => @(47)
		| "X48=" & value: ['a'..'z']+ => @(48)
		| "X49=" & value: ['a'..'z']+ => @(49)
		| "X50=" & value: ['a'..'z']+ => @(50)
		| "X51=" & value: ['a'..'z']+ => @(51)
		| "X52=" & value: ['a'..'z']+ => @(52)
		| "X53=" & value: ['a'..'z']+ => @(53)
		| "X54=" & value: ['a'..'z']+ => @(54)
		| "X55=" & value: ['a'..'z']+ => @(55)
		| "X56=" & value: ['a'..'z']+ => @(56)
		| "X57=" & value: ['a'..'z']+ => @(57)
		| "X58=" & value: ['a'..'z']+ => @(58)
		| "X59=" & value: ['a'..'z']+ => @(59)
		| "X60=" & value: ['a'..'z']+ => @(60)
		| "X61=" & value: ['a'..'z']+ => @(61)
		| "X62=" & value: ['a'..'z']+ => @(62)
		| "X63=" & value: ['a'..'z']+ => @(63)
	parse Start
	""", Direct = false, Portable = false, PrefixTables = true)]
static partial class Tags64Tables;
