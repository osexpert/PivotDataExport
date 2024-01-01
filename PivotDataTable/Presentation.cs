using System.Collections;
using System.Data;

namespace PivotDataTable
{
	public class Presentation<TRow> where TRow : class
	{

		GroupedData<TRow> _data;

		public Presentation(GroupedData<TRow> data)
		{
			_data = data;			
		}

		/// <summary>
		/// FIXME: List of object, Why not ienumerable?
		/// </summary>
		/// <typeparam name="TTableRow"></typeparam>
		/// <param name="toRows"></param>
		/// <returns></returns>
		public Table<TTableRow> GetTableCore<TTableRow>(Func<List<object?[]>, List<TableColumn>, List<TTableRow>> toRows) 
			where TTableRow : class, IEnumerable
		{
			var lastRowGroups = _data.allRowGroups.Last();// OrDefault() ?? [];
			var lastColGroups = _data.allColGroups.Last();// OrDefault() ?? [];

			var colFieldsInSortOrder = _data.fields.Where(f => f.Area == Area.Column)
				.Where(f => f.SortOrder != SortOrder.None)
				.OrderBy(f => f.GroupIndex).ToArray();

			var rowFieldsInSortOrder = _data.fields.Where(f => f.Area == Area.Row)
				.Where(f => f.SortOrder != SortOrder.None)
				.OrderBy(f => f.GroupIndex).ToArray();

			var lastRowGroupsSorted = SortGroups(lastRowGroups, rowFieldsInSortOrder).ToList();

			var lastColGroupsSorted = SortGroups(lastColGroups, colFieldsInSortOrder).ToList();


			// TODO: when writing to json, instead of writing full rows we could write objects........
			// I guess the method could have ended at this point....and some other code could work on this.
			// The code below work on it to produce flat tables.
			// But some other code could produce json nested objects...

			List<object?[]> rows = GetFullRows(_data.dataFields, _data.rowFieldsInGroupOrder, lastRowGroupsSorted, lastColGroupsSorted, out var partialIntersects);

			var tableCols = CreateTableCols(_data.dataFields, _data.rowFieldsInGroupOrder, lastColGroupsSorted);
			//rowsss = SortRows(rowsss, tableCols);

			Table<TTableRow> t = new Table<TTableRow>() { PartialIntersects = partialIntersects };
			t.Rows = toRows(rows, tableCols);
			t.Columns = tableCols;
//			t.RowGroups = _data.rowFieldsInGroupOrder.Select(f => f.ToTableColumn()).ToList();
	//		t.ColumnGroups = _data.colFieldsInGroupOrder.Select(f => f.ToTableColumn()).ToList();

			return t;
		}

		/// <summary>
		/// Add rows with columns: rowGroupCount + (colGroupCount * dataFieldCount)
		/// </summary>
		private List<object?[]> GetFullRows(Field[] dataFields, Field[] rowFieldsInGroupOrder, List<Group<TRow>> lastRowGroups /* sorted */, List<Group<TRow>> lastColGroups /* sorted */, out bool partialIntersects)
		{
			partialIntersects = false;

			List<object?[]> rows = new();

			// does this work without colGroups?
			int colCount = rowFieldsInGroupOrder.Length;
			if (lastColGroups.Any())
				colCount += (lastColGroups.Count * dataFields.Length);
			else
				colCount += dataFields.Length;

			Dictionary<Group<TRow>, int> grpStartIdx = new();
			int totalStartIdx = rowFieldsInGroupOrder.Length;
			foreach (var colGrp in lastColGroups)
			{
				grpStartIdx.Add(colGrp, totalStartIdx);

				totalStartIdx += dataFields.Length;
				// produce name for the colGrp
				// produce starting index in row for colGrp
			}
			
			foreach (var lastRowGroup in lastRowGroups)
			{
				var row = new object?[colCount];

				// TODO: write rowGroup values
				//int rowFieldIdx = 0;
				//foreach (var rowField in rowFieldsInGroupOrder)
				//{
				//	row[rowFieldIdx] = lastRowGroup.GetKeyByField(rowField);
				//	rowFieldIdx++;
				//}

				if (!lastRowGroup.IsRoot)
				{
					var current = lastRowGroup;
					int par_idx = rowFieldsInGroupOrder.Length - 1;
					do
					{
						row[par_idx] = current.Key;
						current = current.ParentGroup;
						par_idx--;
					} while (current != null && !current.IsRoot);
				}

				// this produce one row in the table
				foreach (var lastColGroup in lastColGroups)
				{
					var startIdx = grpStartIdx[lastColGroup];

					if (lastRowGroup.IntersectData.TryGetValue(lastColGroup, out var values))
					{
						//var values = lastRowGroup.IntersectData[lastColGroup];
						// write values
						Array.Copy(values, 0, row, startIdx, values.Length);
					}
					else
					{
						// Use createEmptyIntersects = true if you always want data (instead of lack of data)
						partialIntersects = true;
					}
				}

				rows.Add(row);
			}

			return rows;
		}

#if false
		private List<object?[]> SortRows(List<object?[]> rows, List<TableColumn> tableCols)
		{
			var sortFields = _data.fields
				.Where(f => f.FieldArea != Area.Column) // SortOrder col groups mean SortOrder the columns themself (the labels)
				.Where(f => f.SortOrder != SortOrder.None)
				.OrderBy(f => f.GroupIndex);

			if (sortFields.Any())
			{
				IOrderedEnumerable<object?[]> sorter = null!;
				foreach (var sf in sortFields)
				{
					// TODO lookup idx from filedname
					var sortCol = tableCols.Single(tc => tc.Name == sf.FieldName);
					var idx = tableCols.IndexOf(sortCol);

					if (sorter == null)
						sorter = sf.SortOrder == SortOrder.Asc ? rows.OrderBy(r => r[idx], sf.SortComparer) : rows.OrderByDescending(r => r[idx], sf.SortComparer);
					else
						sorter = sf.SortOrder == SortOrder.Asc ? sorter.ThenBy(r => r[idx], sf.SortComparer) : sorter.ThenByDescending(r => r[idx], sf.SortComparer);
				}
				rows = sorter.ToList();
			}

			return rows;
		}
#endif

		public Table<object?[]> GetTable_Array()
		{
			return GetTableCore((rows, tcols) => rows);
		}

		/*
		 * Example data
		 *     {
      "Region": "Asia",
      "Country": "Maldives",
      "/ItemType:Baby Food/SalesChannel:Offline/OrderPriority": "C, H, L, M",
      "/ItemType:Baby Food/SalesChannel:Offline/OrderDate": "2020-09-08T00:00:00",
      "/ItemType:Baby Food/SalesChannel:Offline/OrderID": "102625882, 103654563, 105711925, 106797683, 107769286, 108995378, 110081136, 111109817, 113167178, 115224540, 116450631, 117479312, 118565070, 119536674, 120622432, 122679793, 123905885, 124934566, 126991927, 129049289, 130135047, 134447181, 136504542, 140816676, 141902434, 143959796, 148271930, 150329291, 151415050, 153612744, 155727183, 157784545, 159841907, 161067998, 162096679, 163182437, 164154040, 165239799, 167297160, 168523252, 169551932, 171609294, 173666656, 174752414, 175921428, 175978505, 177007186, 179064548, 181121909, 185434043, 186519801, 188577163, 192889297, 194946658, 196032416, 200344550, 202401912, 206714046, 207799804, 209857165, 213140618, 214169299, 216226661, 218284022, 219510114, 220538794, 220595872, 221624553, 223681914, 225739276, 227994048, 230051410, 231137168, 233194530, 237506663, 239564025, 240649783, 244961917, 247019279, 251331412, 252417171, 254474532, 258786666, 260844027, 261929786, 262901389, 264127480, 265213239, 266241919, 268299281, 270356643, 271582734, 272611415, 274668776, 275754535, 277811896, 279037988, 280066668, 282124030, 284181392, 285267150, 286493241, 289579284, 291636645, 295948779, 297034537, 299091899, 303404033, 305461394, 306547152, 310859286, 312916648, 316200101, 317228782, 318314540, 319286143, 320371901, 323655354, 324684035, 326741397, 328798758, 329884517, 331053531, 331110608, 332139289, 334196650, 336254012, 340566146, 341651904, 343709266, 348021399, 350078761, 351164519, 355476653, 357534015, 361846148, 362931907, 363903510, 364989268, 368272721, 369301402, 371358764, 373416125, 374642217, 375670897, 375727975, 376756656, 378814017, 380871379, 382097470, 383126151, 385183513, 386269271, 388326632, 389552724, 390581405, 392638766, 394696128, 395781886, 400094020, 402151381, 406463515, 407549273, 408520877, 409606635, 413918769, 415976130, 419259583, 420288264, 420345342, 421374022, 423431384, 425488746, 426714837, 427743518, 429800879, 430886638, 432943999, 434170091, 435198771, 437256133, 439313495, 440399253, 441568267, 441625344, 442654025, 444711387, 446768748, 451080882, 452166640, 454224002, 458536136, 460593497, 465991389, 468048751, 471332204, 472360885, 474418246, 475504004, 478787457, 479816138, 481873500, 483930861, 485156953, 486185634, 486242711, 487271392, 489328753, 491386115, 492612206, 493640887, 495698249, 496784007, 498841369, 503153502, 505210864, 506296622, 510608756, 512666118, 516978251, 518064010, 519035613, 520121371, 524433505, 526490867, 528548228, 529774320, 530803000, 530860078, 531888759, 533946120, 536003482, 537229573, 538258254, 540315616, 541401374, 543458735, 544684827, 545713508, 547770869, 549828231, 550913989, 552140080, 555226123, 557283484, 561595618, 562681376, 563652980, 564738738, 569050872, 571108233, 575420367, 576506125, 578563487, 581846940, 582875621, 584932982, 586018741, 589302194, 590330874, 592388236, 594445598, 595531356, 596700370, 596757447, 597786128, 599843490, 601900851, 606212985, 607298743, 608270347, 609356105, 613668239, 615725600, 620037734, 621123492, 623180854, 627492988, 629550349, 630636107, 633919560, 634948241, 637005603, 639062964, 640289056, 641317737, 641374814, 642403495, 644460856, 646518218, 647744309, 648772990, 650830352, 651916110, 653973472, 655199563, 656228244, 658285605, 660342967, 664655101, 665740859, 667798221, 672110354, 674167716, 675253474, 679565608, 681622970, 684906423, 685935103, 685992181, 687020862, 689078223, 691135585, 692361676, 693390357, 695447719, 696533477, 698590838, 699816930, 700845611, 702902972, 704960334, 706046092, 707215106, 708300864, 710358226, 712415587, 716727721, 718785083, 719870841, 724182975, 726240336, 730552470, 731638228, 733695590, 736979043, 738007724, 740065085, 741150844, 744434297, 745462977, 747520339, 749577701, 750803792, 751832473, 752918231, 754975593, 757032954, 758259046, 759287726, 761345088, 762430846, 763402450, 764488208, 768800342, 770857703, 775169837, 776255595, 778312957, 782625091, 784682452, 785768210, 790080344, 792137706, 794195067, 795421159, 796449840, 796506917, 797535598, 799592959, 801650321, 802876412, 803905093, 805962455, 807048213, 808019816, 809105575, 810331666, 811360347, 813417708, 815475070, 819787204, 820872962, 822930324, 827242457, 829299819, 830385577, 834697711, 836755073, 841067206, 842152965, 844210326, 847493779, 848522460, 850579822, 851665580, 853863275, 854949033, 855977714, 858035075, 860092437, 862347209, 863432967, 864404571, 865490329, 867547690, 871859824, 873917186, 875002944, 879315078, 881372439, 885684573, 886770331, 888827693, 893139827, 895197188, 896282947, 898480641, 899566400, 900595080, 902652442, 904709804, 905935895, 906964576, 908050334, 910107696, 912165057, 913391149, 914419829, 916477191, 918534553, 919620311, 920846402, 921875083, 923932445, 925989806, 930301940, 931387698, 933445060, 937757194, 939814555, 940900313, 945212447, 947269809, 950553262, 951581943, 952667701, 954725062, 956782424, 958008515, 959037196, 961094558, 962180316, 963151919, 964237678, 965463769, 966492450, 968549811, 970607173, 972861945, 974919307, 976005065, 978062427, 982374560, 984431922, 985517680, 989829814, 991887176, 996199309, 997285068, 999342429",
      "/ItemType:Baby Food/SalesChannel:Offline/ShipDate": "2020-10-23T00:00:00",
      "/ItemType:Baby Food/SalesChannel:Offline/UnitsSold": 5678787,
      "/ItemType:Baby Food/SalesChannel:Offline/UnitPrice": 283871.3600000013,
      "/ItemType:Baby Food/SalesChannel:Offline/UnitCost": 177275.04000000245,
      "/ItemType:Baby Food/SalesChannel:Offline/TotalRevenue": 1449680745.3600004,
      "/ItemType:Baby Food/SalesChannel:Offline/TotalCost": 905312223.5399997,
      "/ItemType:Baby Food/SalesChannel:Offline/TotalProfit": 544368521.82,
      "/ItemType:Baby Food/SalesChannel:Offline/RowCount": 1112,
      "/ItemType:Baby Food/SalesChannel:Online/OrderPriority": "C, H, L, M",
      "/ItemType:Baby Food/SalesChannel:Online/OrderDate": "2020-09-10T00:00:00",
      "/ItemType:Baby Food/SalesChannel:Online/OrderID": "100371110, 104683244, 106740605, 107826364, 112138497, 114195859, 118507993, 119593751, 121651113, 125963246, 128020608, 129106366, 130077970, 131304061, 132389819, 133418500, 135475862, 137533223, 138759315, 139787995, 140873754, 141845357, 142931115, 144988477, 146214568, 147243249, 149300611, 151357972, 152443730, 153669822, 154698503, 156755864, 158813226, 163125360, 164211118, 166268479, 170580613, 172637975, 173723733, 178035867, 180093228, 183376681, 184405362, 185491120, 186462724, 187548482, 190831935, 191860616, 193917977, 195975339, 197061097, 198230111, 198287189, 199315869, 201373231, 203430593, 205685365, 207742726, 208828485, 210885846, 215197980, 217255342, 218341100, 222653234, 224710595, 229022729, 230108487, 232165849, 235449302, 236477982, 238535344, 240592706, 241818797, 242904555, 243933236, 245990598, 248047959, 249274051, 250302731, 252360093, 253445851, 255503213, 256729304, 257757985, 259815347, 261872708, 262958467, 267270600, 269327962, 273640096, 274725854, 276783215, 281095349, 283152711, 284238469, 286436164, 287521922, 288550603, 290607964, 292665326, 293891417, 294920098, 296005856, 296977460, 298063218, 300120580, 301346671, 302375352, 304432713, 306490075, 307575833, 308744847, 308801925, 309830605, 311887967, 313945329, 318257462, 319343221, 321400582, 325712716, 327770078, 328855836, 333167970, 335225331, 338508784, 339537465, 340623223, 341594827, 342680585, 345964038, 346992719, 349050080, 351107442, 352333533, 353362214, 353419291, 354447972, 356505334, 358562695, 360817468, 362874829, 363960587, 366017949, 370330083, 372387444, 373473203, 377785336, 379842698, 384154832, 385240590, 386212193, 387297952, 391610085, 393667447, 395724809, 396950900, 397979581, 398036658, 399065339, 401122701, 403180062, 404406154, 405434834, 407492196, 408577954, 410635316, 411861407, 412890088, 414947450, 417004811, 418090569, 419316661, 422402703, 424460065, 428772199, 429857957, 431915318, 436227452, 438284814, 442596948, 443682706, 445740067, 449023520, 450052201, 452109563, 453195321, 456478774, 457507455, 459564816, 461622178, 462707936, 463876950, 463934028, 464962708, 467020070, 469077432, 473389565, 474475324, 476532685, 480844819, 482902181, 483987939, 488300073, 490357434, 494669568, 496726930, 497812688, 501096141, 502124822, 504182183, 506239545, 507465636, 508494317, 508551394, 509580075, 511637437, 513694798, 514920890, 515949571, 518006932, 519092690, 521150052, 522376143, 523404824, 525462186, 527519547, 528605306, 532917439, 534974801, 539286935, 540372693, 541344296, 542430055, 546742188, 548799550, 552083003, 553111684, 553168761, 554197442, 556254804, 558312165, 559538257, 560566937, 562624299, 563710057, 565767419, 566993510, 568022191, 570079553, 572136914, 573222672, 574391686, 574448764, 575477445, 577534806, 579592168, 583904302, 584990060, 585961663, 587047421, 591359555, 593416917, 597729051, 598814809, 600872170, 604155623, 605184304, 607241666, 608327424, 611610877, 612639558, 614696919, 616754281, 617980372, 619009053, 619066131, 620094811, 622152173, 624209535, 625435626, 626464307, 628521668, 629607427, 630579030, 631664788, 635976922, 638034284, 642346417, 643432176, 645489537, 649801671, 651859033, 652944791, 657256925, 659314286, 661371648, 662597739, 663626420, 663683497, 664712178, 666769540, 668826901, 670052993, 671081674, 673139035, 674224793, 676282155, 677508246, 678536927, 680594289, 682651650, 688049542, 690106904, 694419038, 696476399, 697562158, 701874291, 703931653, 708243787, 709329545, 711386907, 714670360, 715699040, 717756402, 718842160, 722125613, 723154294, 725211656, 727269017, 728354775, 729523789, 730609548, 732666909, 734724271, 739036405, 740122163, 741093766, 742179524, 746491658, 748549020, 752861154, 753946912, 756004273, 760316407, 762373769, 763459527, 766742980, 767771661, 769829022, 771886384, 773112475, 774141156, 774198234, 775226914, 777284276, 779341638, 780567729, 781596410, 783653771, 784739530, 785711133, 786796891, 788022983, 789051663, 791109025, 793166387, 797478520, 798564279, 800621640, 804933774, 806991136, 808076894, 812389028, 814446389, 817729842, 818758523, 818815600, 819844281, 821901643, 823959004, 825185096, 826213777, 828271138, 829356896, 830328500, 831414258, 832640349, 833669030, 835726392, 837783753, 840038526, 841124284, 842095887, 843181645, 845239007, 849551141, 851608502, 852694261, 857006394, 859063756, 863375890, 864461648, 866519010, 869802463, 870831143, 872888505, 873974263, 876171958, 877257716, 878286397, 880343759, 882401120, 883627212, 884655892, 885741651, 887799012, 889856374, 891082465, 892111146, 894168508, 896225869, 897311627, 901623761, 903681123, 907993257, 909079015, 911136376, 915448510, 917505872, 918591630, 920789325, 922903764, 924961125, 927018487, 928244578, 929273259, 930359017, 932416379, 934473741, 935699832, 936728513, 938785874, 940843236, 941928994, 943155086, 944183766, 946241128, 948298490, 952610623, 953696382, 955753743, 960065877, 962123239, 963208997, 967521131, 969578492, 973890626, 974976384, 977033746, 980317199, 981345880, 983403241, 984488999, 985460603, 986686694, 987772452, 988801133, 990858495, 992915856, 995170629, 996256387, 997227990, 998313748",
      "/ItemType:Baby Food/SalesChannel:Online/ShipDate": "2020-10-26T00:00:00",
      "/ItemType:Baby Food/SalesChannel:Online/UnitsSold": 5475800,
      "/ItemType:Baby Food/SalesChannel:Online/UnitPrice": 289742.80000000197,
		 * 
		 * */

		/// <summary>
		/// Flat = only one level.
		/// Every row has all columns
		/// </summary>
		/// <returns></returns>
		public Table<KeyValueList> GetTable_FlatKeyValueList_CompleteColumns()
		{
			return GetTableCore((rows, tcols) =>
			{
				List<KeyValueList> dictRows = new();

				foreach (var row in rows)
				{
					//Dictionary<string, object?> dictRow = new();
					var dictRow = new KeyValueList();
					foreach (var v in row.ZipForceEqual(tcols, (f, s) => new { First = f, Second = s }))
						dictRow.Add(v.Second.Name, v.First);

					// perf: to avoid creating one dict per row
					//var dictRow = new KeyValueZipList(row, tcols);

					dictRows.Add(dictRow);
				}

				return dictRows;

			});
		}

		public DataTable GetDataTable()
		{
			var t = GetTable_Array();

			DataTable res = new("row");

			foreach (var f in t.Columns)
			{
				res.Columns.Add(f.Name, f.DataType);
			}

			res.BeginLoadData();
			foreach (var oarr in t.Rows)
				res.LoadDataRow(oarr, fAcceptChanges: false /* ?? */);

//			if (t.GrandTotalRow != null)
//				res.LoadDataRow(t.GrandTotalRow, fAcceptChanges: false /* ?? */);

			res.EndLoadData();

			return res;
		}

		/// <summary>
		/// Variable columns\every row may  have different columns
		/// </summary>
		/// <returns></returns>
		public Table<KeyValueList> GetTable_NestedKeyValueList_VariableColumns()
		{
			bool partialIntersects = false;
			List<KeyValueList> rows = new List<KeyValueList>();

			var lastColGroupsSorted = SortGroups(_data.allColGroups.Last(), _data.colFieldsInGroupOrder).ToList();

			foreach (var rg in SortGroups(_data.allRowGroups.Last(), _data.rowFieldsInGroupOrder))
			{
				KeyValueList r = new();
				rows.Add(r);

				foreach (Group<TRow> parentG in rg.GetParentsAndMe())//includeMeIfRoot: false))
				{
					r.Add(parentG.Field.Name, parentG.Field.GetDisplayValue(parentG.Key));
				}

				Dictionary<Group<TRow>, KeyValueList> groupToKeyVals = null!;
				Dictionary<Group<TRow>, List<KeyValueList>> groupToLists = new();

				// Add the data
				foreach (var cg in lastColGroupsSorted)//SortGroups(_data.allColGroups.Last(), _data.colFieldsInGroupOrder))
				{
					if (rg.IntersectData.TryGetValue(cg, out var data))
					{
						KeyValueList keyVals = GetCreateKeyVals(cg, r, ref groupToKeyVals, groupToLists);

						// dataField order
						foreach (var z in _data.dataFields.ZipForceEqual(data, (f, s) => new { First = f, Second = s }))
						{
							keyVals.Add(z.First.Name, z.First.GetDisplayValue(z.Second));
						}
					}
					else
					{
						partialIntersects = true;
					}
				}
			}

			var tableCols = CreateTableCols(_data.dataFields, _data.rowFieldsInGroupOrder, lastColGroupsSorted);

			return new Table<KeyValueList>() { Rows = rows.ToList(), PartialRows = partialIntersects, PartialIntersects = partialIntersects, Columns = tableCols };
		}

		private KeyValueList GetCreateKeyVals(Group<TRow> cg, KeyValueList r, ref Dictionary<Group<TRow>, KeyValueList> groupToKeyVals, Dictionary<Group<TRow>, List<KeyValueList>> groupToLists)
		{
			if (cg.IsRoot)
			{
				return r;
			}

			KeyValueList keyVals = null!;

			foreach (var colGrp in cg.GetParentsAndMe())//includeMeIfRoot: true))
			{
				// add the root group
				if (groupToKeyVals == null)
				{
					groupToKeyVals = new();
					groupToKeyVals.Add(colGrp.ParentGroup!, r);
					if (!colGrp.ParentGroup!.IsRoot)
						throw new Exception("not root");
				}

				if (!groupToLists.TryGetValue(colGrp.ParentGroup!, out var list))
				{
					list = new();
					groupToLists.Add(colGrp.ParentGroup!, list);

					var parKeyVals = groupToKeyVals[colGrp.ParentGroup!];
					parKeyVals.Add(colGrp.Field.Name + "List", list);
				}

				if (!groupToKeyVals.TryGetValue(colGrp, out keyVals!))
				{
					keyVals = new KeyValueList();
					keyVals.Add(colGrp.Field.Name, colGrp.Field.GetDisplayValue(colGrp.Key));

					list.Add(keyVals);

					groupToKeyVals.Add(colGrp, keyVals);
				}

			}

			return keyVals;
		}

		public class TableGroup
		{
			public string Name { get; set; } = null!;
			//public object DataType { get; set; }

			public object? Value { get; set; }

			//public TableGroup Parent { get; set; }
		}


		public Func<IEnumerable<TableGroup>, string, string> ColumnNameGenerator = SlashedColumnNameGeneratorWithFieldNames;

		public static string SlashedColumnNameGeneratorWithFieldNames(IEnumerable<TableGroup> tgs, string dataField)
		{
			// /Country:USA/Region:Florida/CarCount
			var middle = string.Join("/", tgs.Select(tg => $"{Escaper.Escape(tg.Name)}:{Escaper.Escape(Convert.ToString(tg.Value) ?? string.Empty)}"));
			var combName = $"/{middle}/{Escaper.Escape(dataField)}";
			return combName;
		}

		public static string SlashedColumnNameGenerator(IEnumerable<TableGroup> tgs, string dataField)
		{
			// TODO: escape?
			// /USA/Florida/CarCount
			var middle = string.Join("/", tgs.Select(tg => tg.Value));
			var combName = $"/{middle}.{dataField}";
			return combName;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="tgs"></param>
		/// <param name="dataField"></param>
		/// <returns></returns>
		public static string DottedColumnNameGenerator(IEnumerable<TableGroup> tgs, string dataField)
		{
			// TODO: escape?
			// USA.Florida.CarCount
			var middle = string.Join(".", tgs.Select(tg => tg.Value));
			var combName = $"{middle}.{dataField}";
			return combName;
		}

		private List<TableColumn> CreateTableCols(Field[] dataFields, Field[] rowGroupFields, List<Group<TRow>> lastColGroups /* sorted */)
		{
			List<TableColumn> tablecols = new();
			// fill rowGroups
			//			int colCount = rowGroupFields.Length + (lastColGroups.Count * dataFields.Length);
			tablecols.AddRange(rowGroupFields.Select(f => f.ToTableColumn()));

			List<TableColumn> tablecols_after = new();
			// dont get this logix...
			foreach (var gr in lastColGroups)
			{
				if (gr.IsRoot)
				{
					tablecols_after.AddRange(dataFields.Select(df => df.ToTableColumn()));
				}
				else
				{
					Stack<TableGroup> tgs = new();


					var parent = gr;
					do
					{
						tgs.Push(new TableGroup
						{
							Name = parent.Field.Name,
							//DataType = parent.Field.DataType,
							Value = parent.Key
						});

						parent = parent.ParentGroup;
					} while (parent != null && !parent.IsRoot);

					foreach (var dataField in dataFields)
					{
						var combName = ColumnNameGenerator(tgs, dataField.Name);

						tablecols_after.Add(dataField.ToTableColumn(combName, tgs.Select(tg => tg.Value).ToArray()));
					}
				}
			}

			//			tablecols_after = SortColGroupsCols(tablecols_after, colGroupFields);

			tablecols.AddRange(tablecols_after);

			return tablecols;
		}


		private IEnumerable<TEle> SortGroups<TEle>(IEnumerable<TEle> grops, Field[] groupFields) where TEle : Group<TRow>
		{
			return SortGroups<TEle>(grops, groupFields, ele => ele);
		}

		/// <summary>
		/// Sort the last group level.
		/// Sort by checking parent values
		/// </summary>
		private IEnumerable<TEle> SortGroups<TEle>(IEnumerable<TEle> grops, Field[] groupFields, Func<TEle, Group<TRow>> getGroup)
		{
			//.OrderBy(a => a.Key.Groups[0]).ThenBy(a => a.Key.Groups[1]).ToList();

			//var sortFields = _fields.Where(f => f.Grouping == Grouping.Col)
			//	.Where(f => f.SortOrder != SortOrder.None)
			//	.OrderBy(f => f.SortIndex)
			//	.ToArray();

			var sortedGroupFields = groupFields.Where(f => f.SortOrder != SortOrder.None);

			if (sortedGroupFields.Any())
			{
				IOrderedEnumerable<TEle> sorter = null!;

				int colFieldIdx = 0;
				foreach (var colField in sortedGroupFields)
				{
					int colFieldIdx_local_capture = colFieldIdx;

					if (sorter == null)
						sorter = colField.SortOrder == SortOrder.Asc ?
							grops.OrderBy(r => getGroup(r).GetKeyByField(colField), colField.SortComparer)
							: grops.OrderByDescending(r => getGroup(r).GetKeyByField(colField), colField.SortComparer);
					else
						sorter = colField.SortOrder == SortOrder.Asc ?
							sorter.ThenBy(r => getGroup(r).GetKeyByField(colField), colField.SortComparer)
							: sorter.ThenByDescending(r => getGroup(r).GetKeyByField(colField), colField.SortComparer);

					colFieldIdx++;
				}

				grops = sorter;//.ToList(); // tolist needed?
			}

			return grops;
		}
	}
}
