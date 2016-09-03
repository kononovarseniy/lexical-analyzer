using LexicalAnalyzer.Utils;
using System.Collections.Generic;

namespace LexicalAnalyzer.RegularExpressions
{
    public partial class RegexParser
    {
        private static readonly Dictionary<char, char> EscapeMap = new Dictionary<char, char>()
        {
            ['a'] = '\a',
            ['b'] = '\b',
            ['t'] = '\t',
            ['r'] = '\r',
            ['v'] = '\v',
            ['f'] = '\f',
            ['n'] = '\n',
            ['.'] = '.',
            ['-'] = '-',
            ['^'] = '^',
            ['['] = '[',
            [']'] = ']',
            ['\\'] = '\\'
        };

        // Initialized in static constructor.
        private static readonly Dictionary<char, IntSet> BuiltinCharClasses;

        public static readonly IntSet All = IntSet.All;

        #region White spaces
        public static readonly IntSet WhiteSpaces =
            new IntSet(0x0009, 0x000D) |
            new IntSet(0x0020) |
            new IntSet(0x0085) |
            new IntSet(0x00A0) |
            new IntSet(0x1680) |
            new IntSet(0x2000, 0x200A) |
            new IntSet(0x2028) |
            new IntSet(0x2029) |
            new IntSet(0x202F) |
            new IntSet(0x205F) |
            new IntSet(0x3000);

        #endregion

        public static readonly IntSet Digits = new IntSet(0x0030, 0x0039);

        #region Letters
        public static readonly IntSet Letters =
            new IntSet(65, 90) |
            new IntSet(97, 122) |
            new IntSet(170) |
            new IntSet(181) |
            new IntSet(186) |
            new IntSet(192, 214) |
            new IntSet(216, 246) |
            new IntSet(248, 705) |
            new IntSet(710, 721) |
            new IntSet(736, 740) |
            new IntSet(748) |
            new IntSet(750) |
            new IntSet(880, 884) |
            new IntSet(886, 887) |
            new IntSet(890, 893) |
            new IntSet(895) |
            new IntSet(902) |
            new IntSet(904, 906) |
            new IntSet(908) |
            new IntSet(910, 929) |
            new IntSet(931, 1013) |
            new IntSet(1015, 1153) |
            new IntSet(1162, 1327) |
            new IntSet(1329, 1366) |
            new IntSet(1369) |
            new IntSet(1377, 1415) |
            new IntSet(1488, 1514) |
            new IntSet(1520, 1522) |
            new IntSet(1568, 1610) |
            new IntSet(1646, 1647) |
            new IntSet(1649, 1747) |
            new IntSet(1749) |
            new IntSet(1765, 1766) |
            new IntSet(1774, 1775) |
            new IntSet(1786, 1788) |
            new IntSet(1791) |
            new IntSet(1808) |
            new IntSet(1810, 1839) |
            new IntSet(1869, 1957) |
            new IntSet(1969) |
            new IntSet(1994, 2026) |
            new IntSet(2036, 2037) |
            new IntSet(2042) |
            new IntSet(2048, 2069) |
            new IntSet(2074) |
            new IntSet(2084) |
            new IntSet(2088) |
            new IntSet(2112, 2136) |
            new IntSet(2208, 2228) |
            new IntSet(2230, 2237) |
            new IntSet(2308, 2361) |
            new IntSet(2365) |
            new IntSet(2384) |
            new IntSet(2392, 2401) |
            new IntSet(2417, 2432) |
            new IntSet(2437, 2444) |
            new IntSet(2447, 2448) |
            new IntSet(2451, 2472) |
            new IntSet(2474, 2480) |
            new IntSet(2482) |
            new IntSet(2486, 2489) |
            new IntSet(2493) |
            new IntSet(2510) |
            new IntSet(2524, 2525) |
            new IntSet(2527, 2529) |
            new IntSet(2544, 2545) |
            new IntSet(2565, 2570) |
            new IntSet(2575, 2576) |
            new IntSet(2579, 2600) |
            new IntSet(2602, 2608) |
            new IntSet(2610, 2611) |
            new IntSet(2613, 2614) |
            new IntSet(2616, 2617) |
            new IntSet(2649, 2652) |
            new IntSet(2654) |
            new IntSet(2674, 2676) |
            new IntSet(2693, 2701) |
            new IntSet(2703, 2705) |
            new IntSet(2707, 2728) |
            new IntSet(2730, 2736) |
            new IntSet(2738, 2739) |
            new IntSet(2741, 2745) |
            new IntSet(2749) |
            new IntSet(2768) |
            new IntSet(2784, 2785) |
            new IntSet(2809) |
            new IntSet(2821, 2828) |
            new IntSet(2831, 2832) |
            new IntSet(2835, 2856) |
            new IntSet(2858, 2864) |
            new IntSet(2866, 2867) |
            new IntSet(2869, 2873) |
            new IntSet(2877) |
            new IntSet(2908, 2909) |
            new IntSet(2911, 2913) |
            new IntSet(2929) |
            new IntSet(2947) |
            new IntSet(2949, 2954) |
            new IntSet(2958, 2960) |
            new IntSet(2962, 2965) |
            new IntSet(2969, 2970) |
            new IntSet(2972) |
            new IntSet(2974, 2975) |
            new IntSet(2979, 2980) |
            new IntSet(2984, 2986) |
            new IntSet(2990, 3001) |
            new IntSet(3024) |
            new IntSet(3077, 3084) |
            new IntSet(3086, 3088) |
            new IntSet(3090, 3112) |
            new IntSet(3114, 3129) |
            new IntSet(3133) |
            new IntSet(3160, 3162) |
            new IntSet(3168, 3169) |
            new IntSet(3200) |
            new IntSet(3205, 3212) |
            new IntSet(3214, 3216) |
            new IntSet(3218, 3240) |
            new IntSet(3242, 3251) |
            new IntSet(3253, 3257) |
            new IntSet(3261) |
            new IntSet(3294) |
            new IntSet(3296, 3297) |
            new IntSet(3313, 3314) |
            new IntSet(3333, 3340) |
            new IntSet(3342, 3344) |
            new IntSet(3346, 3386) |
            new IntSet(3389) |
            new IntSet(3406) |
            new IntSet(3412, 3414) |
            new IntSet(3423, 3425) |
            new IntSet(3450, 3455) |
            new IntSet(3461, 3478) |
            new IntSet(3482, 3505) |
            new IntSet(3507, 3515) |
            new IntSet(3517) |
            new IntSet(3520, 3526) |
            new IntSet(3585, 3632) |
            new IntSet(3634, 3635) |
            new IntSet(3648, 3654) |
            new IntSet(3713, 3714) |
            new IntSet(3716) |
            new IntSet(3719, 3720) |
            new IntSet(3722) |
            new IntSet(3725) |
            new IntSet(3732, 3735) |
            new IntSet(3737, 3743) |
            new IntSet(3745, 3747) |
            new IntSet(3749) |
            new IntSet(3751) |
            new IntSet(3754, 3755) |
            new IntSet(3757, 3760) |
            new IntSet(3762, 3763) |
            new IntSet(3773) |
            new IntSet(3776, 3780) |
            new IntSet(3782) |
            new IntSet(3804, 3807) |
            new IntSet(3840) |
            new IntSet(3904, 3911) |
            new IntSet(3913, 3948) |
            new IntSet(3976, 3980) |
            new IntSet(4096, 4138) |
            new IntSet(4159) |
            new IntSet(4176, 4181) |
            new IntSet(4186, 4189) |
            new IntSet(4193) |
            new IntSet(4197, 4198) |
            new IntSet(4206, 4208) |
            new IntSet(4213, 4225) |
            new IntSet(4238) |
            new IntSet(4256, 4293) |
            new IntSet(4295) |
            new IntSet(4301) |
            new IntSet(4304, 4346) |
            new IntSet(4348, 4680) |
            new IntSet(4682, 4685) |
            new IntSet(4688, 4694) |
            new IntSet(4696) |
            new IntSet(4698, 4701) |
            new IntSet(4704, 4744) |
            new IntSet(4746, 4749) |
            new IntSet(4752, 4784) |
            new IntSet(4786, 4789) |
            new IntSet(4792, 4798) |
            new IntSet(4800) |
            new IntSet(4802, 4805) |
            new IntSet(4808, 4822) |
            new IntSet(4824, 4880) |
            new IntSet(4882, 4885) |
            new IntSet(4888, 4954) |
            new IntSet(4992, 5007) |
            new IntSet(5024, 5109) |
            new IntSet(5112, 5117) |
            new IntSet(5121, 5740) |
            new IntSet(5743, 5759) |
            new IntSet(5761, 5786) |
            new IntSet(5792, 5866) |
            new IntSet(5873, 5880) |
            new IntSet(5888, 5900) |
            new IntSet(5902, 5905) |
            new IntSet(5920, 5937) |
            new IntSet(5952, 5969) |
            new IntSet(5984, 5996) |
            new IntSet(5998, 6000) |
            new IntSet(6016, 6067) |
            new IntSet(6103) |
            new IntSet(6108) |
            new IntSet(6176, 6263) |
            new IntSet(6272, 6276) |
            new IntSet(6279, 6312) |
            new IntSet(6314) |
            new IntSet(6320, 6389) |
            new IntSet(6400, 6430) |
            new IntSet(6480, 6509) |
            new IntSet(6512, 6516) |
            new IntSet(6528, 6571) |
            new IntSet(6576, 6601) |
            new IntSet(6656, 6678) |
            new IntSet(6688, 6740) |
            new IntSet(6823) |
            new IntSet(6917, 6963) |
            new IntSet(6981, 6987) |
            new IntSet(7043, 7072) |
            new IntSet(7086, 7087) |
            new IntSet(7098, 7141) |
            new IntSet(7168, 7203) |
            new IntSet(7245, 7247) |
            new IntSet(7258, 7293) |
            new IntSet(7296, 7304) |
            new IntSet(7401, 7404) |
            new IntSet(7406, 7409) |
            new IntSet(7413, 7414) |
            new IntSet(7424, 7615) |
            new IntSet(7680, 7957) |
            new IntSet(7960, 7965) |
            new IntSet(7968, 8005) |
            new IntSet(8008, 8013) |
            new IntSet(8016, 8023) |
            new IntSet(8025) |
            new IntSet(8027) |
            new IntSet(8029) |
            new IntSet(8031, 8061) |
            new IntSet(8064, 8116) |
            new IntSet(8118, 8124) |
            new IntSet(8126) |
            new IntSet(8130, 8132) |
            new IntSet(8134, 8140) |
            new IntSet(8144, 8147) |
            new IntSet(8150, 8155) |
            new IntSet(8160, 8172) |
            new IntSet(8178, 8180) |
            new IntSet(8182, 8188) |
            new IntSet(8305) |
            new IntSet(8319) |
            new IntSet(8336, 8348) |
            new IntSet(8450) |
            new IntSet(8455) |
            new IntSet(8458, 8467) |
            new IntSet(8469) |
            new IntSet(8473, 8477) |
            new IntSet(8484) |
            new IntSet(8486) |
            new IntSet(8488) |
            new IntSet(8490, 8493) |
            new IntSet(8495, 8505) |
            new IntSet(8508, 8511) |
            new IntSet(8517, 8521) |
            new IntSet(8526) |
            new IntSet(8579, 8580) |
            new IntSet(11264, 11310) |
            new IntSet(11312, 11358) |
            new IntSet(11360, 11492) |
            new IntSet(11499, 11502) |
            new IntSet(11506, 11507) |
            new IntSet(11520, 11557) |
            new IntSet(11559) |
            new IntSet(11565) |
            new IntSet(11568, 11623) |
            new IntSet(11631) |
            new IntSet(11648, 11670) |
            new IntSet(11680, 11686) |
            new IntSet(11688, 11694) |
            new IntSet(11696, 11702) |
            new IntSet(11704, 11710) |
            new IntSet(11712, 11718) |
            new IntSet(11720, 11726) |
            new IntSet(11728, 11734) |
            new IntSet(11736, 11742) |
            new IntSet(11823) |
            new IntSet(12293, 12294) |
            new IntSet(12337, 12341) |
            new IntSet(12347, 12348) |
            new IntSet(12353, 12438) |
            new IntSet(12445, 12447) |
            new IntSet(12449, 12538) |
            new IntSet(12540, 12543) |
            new IntSet(12549, 12589) |
            new IntSet(12593, 12686) |
            new IntSet(12704, 12730) |
            new IntSet(12784, 12799) |
            new IntSet(13312) |
            new IntSet(19893) |
            new IntSet(19968) |
            new IntSet(40917) |
            new IntSet(40960, 42124) |
            new IntSet(42192, 42237) |
            new IntSet(42240, 42508) |
            new IntSet(42512, 42527) |
            new IntSet(42538, 42539) |
            new IntSet(42560, 42606) |
            new IntSet(42623, 42653) |
            new IntSet(42656, 42725) |
            new IntSet(42775, 42783) |
            new IntSet(42786, 42888) |
            new IntSet(42891, 42926) |
            new IntSet(42928, 42935) |
            new IntSet(42999, 43009) |
            new IntSet(43011, 43013) |
            new IntSet(43015, 43018) |
            new IntSet(43020, 43042) |
            new IntSet(43072, 43123) |
            new IntSet(43138, 43187) |
            new IntSet(43250, 43255) |
            new IntSet(43259) |
            new IntSet(43261) |
            new IntSet(43274, 43301) |
            new IntSet(43312, 43334) |
            new IntSet(43360, 43388) |
            new IntSet(43396, 43442) |
            new IntSet(43471) |
            new IntSet(43488, 43492) |
            new IntSet(43494, 43503) |
            new IntSet(43514, 43518) |
            new IntSet(43520, 43560) |
            new IntSet(43584, 43586) |
            new IntSet(43588, 43595) |
            new IntSet(43616, 43638) |
            new IntSet(43642) |
            new IntSet(43646, 43695) |
            new IntSet(43697) |
            new IntSet(43701, 43702) |
            new IntSet(43705, 43709) |
            new IntSet(43712) |
            new IntSet(43714) |
            new IntSet(43739, 43741) |
            new IntSet(43744, 43754) |
            new IntSet(43762, 43764) |
            new IntSet(43777, 43782) |
            new IntSet(43785, 43790) |
            new IntSet(43793, 43798) |
            new IntSet(43808, 43814) |
            new IntSet(43816, 43822) |
            new IntSet(43824, 43866) |
            new IntSet(43868, 43877) |
            new IntSet(43888, 44002) |
            new IntSet(44032) |
            new IntSet(55203) |
            new IntSet(55216, 55238) |
            new IntSet(55243, 55291) |
            new IntSet(63744, 64109) |
            new IntSet(64112, 64217) |
            new IntSet(64256, 64262) |
            new IntSet(64275, 64279) |
            new IntSet(64285) |
            new IntSet(64287, 64296) |
            new IntSet(64298, 64310) |
            new IntSet(64312, 64316) |
            new IntSet(64318) |
            new IntSet(64320, 64321) |
            new IntSet(64323, 64324) |
            new IntSet(64326, 64433) |
            new IntSet(64467, 64829) |
            new IntSet(64848, 64911) |
            new IntSet(64914, 64967) |
            new IntSet(65008, 65019) |
            new IntSet(65136, 65140) |
            new IntSet(65142, 65276) |
            new IntSet(65313, 65338) |
            new IntSet(65345, 65370) |
            new IntSet(65382, 65470) |
            new IntSet(65474, 65479) |
            new IntSet(65482, 65487) |
            new IntSet(65490, 65495) |
            new IntSet(65498, 65500) |
            new IntSet(65536, 65547) |
            new IntSet(65549, 65574) |
            new IntSet(65576, 65594) |
            new IntSet(65596, 65597) |
            new IntSet(65599, 65613) |
            new IntSet(65616, 65629) |
            new IntSet(65664, 65786) |
            new IntSet(66176, 66204) |
            new IntSet(66208, 66256) |
            new IntSet(66304, 66335) |
            new IntSet(66352, 66368) |
            new IntSet(66370, 66377) |
            new IntSet(66384, 66421) |
            new IntSet(66432, 66461) |
            new IntSet(66464, 66499) |
            new IntSet(66504, 66511) |
            new IntSet(66560, 66717) |
            new IntSet(66736, 66771) |
            new IntSet(66776, 66811) |
            new IntSet(66816, 66855) |
            new IntSet(66864, 66915) |
            new IntSet(67072, 67382) |
            new IntSet(67392, 67413) |
            new IntSet(67424, 67431) |
            new IntSet(67584, 67589) |
            new IntSet(67592) |
            new IntSet(67594, 67637) |
            new IntSet(67639, 67640) |
            new IntSet(67644) |
            new IntSet(67647, 67669) |
            new IntSet(67680, 67702) |
            new IntSet(67712, 67742) |
            new IntSet(67808, 67826) |
            new IntSet(67828, 67829) |
            new IntSet(67840, 67861) |
            new IntSet(67872, 67897) |
            new IntSet(67968, 68023) |
            new IntSet(68030, 68031) |
            new IntSet(68096) |
            new IntSet(68112, 68115) |
            new IntSet(68117, 68119) |
            new IntSet(68121, 68147) |
            new IntSet(68192, 68220) |
            new IntSet(68224, 68252) |
            new IntSet(68288, 68295) |
            new IntSet(68297, 68324) |
            new IntSet(68352, 68405) |
            new IntSet(68416, 68437) |
            new IntSet(68448, 68466) |
            new IntSet(68480, 68497) |
            new IntSet(68608, 68680) |
            new IntSet(68736, 68786) |
            new IntSet(68800, 68850) |
            new IntSet(69635, 69687) |
            new IntSet(69763, 69807) |
            new IntSet(69840, 69864) |
            new IntSet(69891, 69926) |
            new IntSet(69968, 70002) |
            new IntSet(70006) |
            new IntSet(70019, 70066) |
            new IntSet(70081, 70084) |
            new IntSet(70106) |
            new IntSet(70108) |
            new IntSet(70144, 70161) |
            new IntSet(70163, 70187) |
            new IntSet(70272, 70278) |
            new IntSet(70280) |
            new IntSet(70282, 70285) |
            new IntSet(70287, 70301) |
            new IntSet(70303, 70312) |
            new IntSet(70320, 70366) |
            new IntSet(70405, 70412) |
            new IntSet(70415, 70416) |
            new IntSet(70419, 70440) |
            new IntSet(70442, 70448) |
            new IntSet(70450, 70451) |
            new IntSet(70453, 70457) |
            new IntSet(70461) |
            new IntSet(70480) |
            new IntSet(70493, 70497) |
            new IntSet(70656, 70708) |
            new IntSet(70727, 70730) |
            new IntSet(70784, 70831) |
            new IntSet(70852, 70853) |
            new IntSet(70855) |
            new IntSet(71040, 71086) |
            new IntSet(71128, 71131) |
            new IntSet(71168, 71215) |
            new IntSet(71236) |
            new IntSet(71296, 71338) |
            new IntSet(71424, 71449) |
            new IntSet(71840, 71903) |
            new IntSet(71935) |
            new IntSet(72384, 72440) |
            new IntSet(72704, 72712) |
            new IntSet(72714, 72750) |
            new IntSet(72768) |
            new IntSet(72818, 72847) |
            new IntSet(73728, 74649) |
            new IntSet(74880, 75075) |
            new IntSet(77824, 78894) |
            new IntSet(82944, 83526) |
            new IntSet(92160, 92728) |
            new IntSet(92736, 92766) |
            new IntSet(92880, 92909) |
            new IntSet(92928, 92975) |
            new IntSet(92992, 92995) |
            new IntSet(93027, 93047) |
            new IntSet(93053, 93071) |
            new IntSet(93952, 94020) |
            new IntSet(94032) |
            new IntSet(94099, 94111) |
            new IntSet(94176) |
            new IntSet(94208) |
            new IntSet(100332) |
            new IntSet(100352, 101106) |
            new IntSet(110592, 110593) |
            new IntSet(113664, 113770) |
            new IntSet(113776, 113788) |
            new IntSet(113792, 113800) |
            new IntSet(113808, 113817) |
            new IntSet(119808, 119892) |
            new IntSet(119894, 119964) |
            new IntSet(119966, 119967) |
            new IntSet(119970) |
            new IntSet(119973, 119974) |
            new IntSet(119977, 119980) |
            new IntSet(119982, 119993) |
            new IntSet(119995) |
            new IntSet(119997, 120003) |
            new IntSet(120005, 120069) |
            new IntSet(120071, 120074) |
            new IntSet(120077, 120084) |
            new IntSet(120086, 120092) |
            new IntSet(120094, 120121) |
            new IntSet(120123, 120126) |
            new IntSet(120128, 120132) |
            new IntSet(120134) |
            new IntSet(120138, 120144) |
            new IntSet(120146, 120485) |
            new IntSet(120488, 120512) |
            new IntSet(120514, 120538) |
            new IntSet(120540, 120570) |
            new IntSet(120572, 120596) |
            new IntSet(120598, 120628) |
            new IntSet(120630, 120654) |
            new IntSet(120656, 120686) |
            new IntSet(120688, 120712) |
            new IntSet(120714, 120744) |
            new IntSet(120746, 120770) |
            new IntSet(120772, 120779) |
            new IntSet(124928, 125124) |
            new IntSet(125184, 125251) |
            new IntSet(126464, 126467) |
            new IntSet(126469, 126495) |
            new IntSet(126497, 126498) |
            new IntSet(126500) |
            new IntSet(126503) |
            new IntSet(126505, 126514) |
            new IntSet(126516, 126519) |
            new IntSet(126521) |
            new IntSet(126523) |
            new IntSet(126530) |
            new IntSet(126535) |
            new IntSet(126537) |
            new IntSet(126539) |
            new IntSet(126541, 126543) |
            new IntSet(126545, 126546) |
            new IntSet(126548) |
            new IntSet(126551) |
            new IntSet(126553) |
            new IntSet(126555) |
            new IntSet(126557) |
            new IntSet(126559) |
            new IntSet(126561, 126562) |
            new IntSet(126564) |
            new IntSet(126567, 126570) |
            new IntSet(126572, 126578) |
            new IntSet(126580, 126583) |
            new IntSet(126585, 126588) |
            new IntSet(126590) |
            new IntSet(126592, 126601) |
            new IntSet(126603, 126619) |
            new IntSet(126625, 126627) |
            new IntSet(126629, 126633) |
            new IntSet(126635, 126651) |
            new IntSet(131072) |
            new IntSet(173782) |
            new IntSet(173824) |
            new IntSet(177972) |
            new IntSet(177984) |
            new IntSet(178205) |
            new IntSet(178208) |
            new IntSet(183969) |
            new IntSet(194560, 195101);
        #endregion

        public static readonly IntSet DigitsAndLetters = Digits | Letters;

        static RegexParser()
        {
            BuiltinCharClasses = new Dictionary<char, IntSet>()
            {
                ['s'] = WhiteSpaces,
                ['S'] = ~WhiteSpaces,
                ['d'] = Digits,
                ['D'] = ~Digits,
                ['l'] = Letters,
                ['L'] = ~Letters,
                ['w'] = DigitsAndLetters,
                ['W'] = ~DigitsAndLetters,
            };
        }
    }
}
