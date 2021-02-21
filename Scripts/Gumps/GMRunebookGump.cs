using System;
using System.Collections;
using Server;
using Server.Items;
using Server.Network;
using Server.Prompts;

namespace Server.Gumps
{
	public class GMRunebookGump : Gump
	{
		private GMRunebook m_Book;

		public GMRunebook Book{ get{ return m_Book; } }

		public int GetMapHue( Map map )
		{
			if ( map == Map.Trammel ) return 10;
			else if ( map == Map.Felucca ) return 81;
			else if ( map == Map.Ilshenar ) return 1102;
			else if ( map == Map.Malas ) return 1102;
			else if ( map == Map.Tokuno ) return 123;
			return 0;
		}

		public string GetName( string name )
		{
			if ( name == null || (name = name.Trim()).Length <= 0 ) return "(indescript)";
			return name;
		}

		private void AddBackground()
		{
			AddPage( 0 );

			AddImage( 100, 10, 2200 );

			for ( int i = 0; i < 2; ++i )
			{
				int xOffset = 125 + (i * 165);
				AddImage( xOffset, 50, 57 );
				xOffset += 20;
				for ( int j = 0; j < 6; ++j, xOffset += 15 )
					AddImage( xOffset, 50, 58 );
				AddImage( xOffset - 5, 50, 59 );
			}

			for ( int i = 0, xOffset = 130, gumpID = 2225; i < 4; ++i, xOffset += 35, ++gumpID )
				AddButton( xOffset, 187, gumpID, gumpID, 0, GumpButtonType.Page, 2 + i );
			for ( int i = 0, xOffset = 300, gumpID = 2229; i < 4; ++i, xOffset += 35, ++gumpID )
				AddButton( xOffset, 187, gumpID, gumpID, 0, GumpButtonType.Page, 6 + i );
			AddHtmlLocalized( 140, 40,  80, 18, 1011296, false, false );
			AddHtml( 210, 40, 50, 18, m_Book.CurCharges.ToString(), false, false );
			AddHtmlLocalized( 300, 40, 100, 18, 1011297, false, false );
			AddHtml( 380, 40, 50, 18, m_Book.MaxCharges.ToString(), false, false );
		}

		private void AddIndex()
		{
			AddPage( 1 );
			AddButton( 125, 15, 2472, 2473, 1, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 158, 22, 100, 18, 1011299, false, false );
			ArrayList entries = m_Book.Entries;
			for ( int i = 0; i < 16; ++i )
			{
				string desc;
				int hue;
				if ( i < entries.Count )
				{
					desc = GetName( ((GMRunebookEntry)entries[i]).Description );
					hue = GetMapHue( ((GMRunebookEntry)entries[i]).Map );
				}
				else
				{
					desc = "Empty";
					hue = 0;
				}
				AddButton( 130 + ((i / 8) * 160), 65 + ((i % 8) * 15), 2103, 2104, 2 + (i * 6) + 0, GumpButtonType.Reply, 0 );
				AddLabelCropped( 145 + ((i / 8) * 160), 60 + ((i % 8) * 15), 115, 17, hue, desc );
			}
			AddButton( 393, 14, 2206, 2206, 0, GumpButtonType.Page, 2 );
		}

		private void AddDetails( int index, int half )
		{
			AddButton( 130 + (half * 160), 65, 2103, 2104, 2 + (index * 6) + 0, GumpButtonType.Reply, 0 );
			string desc;
			int hue;
			if ( index < m_Book.Entries.Count )
			{
				GMRunebookEntry e = (GMRunebookEntry)m_Book.Entries[index];
				desc = GetName( e.Description );
				hue = GetMapHue( e.Map );
				AddLabel( 135 + (half * 160), 80, 0, String.Format( "x{0} y{1} z{2}", e.Location.X, e.Location.Y, e.Location.Z ) );
				AddLabel( 135 + (half * 160), 95, 0, String.Format( "{0}", e.Map ) );
				AddButton( 135 + (half * 160), 115, 2437, 2438, 2 + (index * 6) + 1, GumpButtonType.Reply, 0 );
				AddHtmlLocalized( 150 + (half * 160), 115, 100, 18, 1011298, false, false );
				if ( e != m_Book.Default )
				{
					AddButton( 160 + (half * 140), 20, 2361, 2361, 2 + (index * 6) + 2, GumpButtonType.Reply, 0 );
					AddHtmlLocalized( 175 + (half * 140), 15, 100, 18, 1011300, false, false );
				}
				AddButton( 135 + (half * 160), 140, 2103, 2104, 2 + (index * 6) + 3, GumpButtonType.Reply, 0 );
				AddHtmlLocalized( 150 + (half * 160), 136, 110, 20, 1062722, false, false );
				AddButton( 135 + (half * 160), 158, 2103, 2104, 2 + (index * 6) + 4, GumpButtonType.Reply, 0 );
				AddHtmlLocalized( 150 + (half * 160), 154, 110, 20, 1062723, false, false );
			}
			else
			{
				desc = "Empty";
				hue = 0;
			}
			AddLabelCropped( 145 + (half * 160), 60, 115, 17, hue, desc );
		}

		public GMRunebookGump( Mobile from, GMRunebook book ) : base( 150, 200 )
		{
			m_Book = book;
			AddBackground();
			AddIndex();
			for ( int page = 0; page < 8; ++page )
			{
				AddPage( 2 + page );
				AddButton( 125, 14, 2205, 2205, 0, GumpButtonType.Page, 1 + page );
				if ( page < 7 ) AddButton( 393, 14, 2206, 2206, 0, GumpButtonType.Page, 3 + page );
				for ( int half = 0; half < 2; ++half )
					AddDetails( (page * 2) + half, half );
			}
		}

		private class InternalPrompt : Prompt
		{
			private GMRunebook m_Book;
			public InternalPrompt( GMRunebook book )
			{
				m_Book = book;
			}

			public override void OnResponse( Mobile from, string text )
			{
				if ( m_Book.Deleted || !from.InRange( m_Book.GetWorldLocation(), 1 ) ) return;
				if ( m_Book.CheckAccess( from ) )
				{
					m_Book.Description = Utility.FixHtml( text.Trim() );
					from.CloseGump( typeof( GMRunebookGump ) );
					from.SendGump( new GMRunebookGump( from, m_Book ) );
					from.SendMessage( "The book's title has been changed." );
				}
				else from.SendLocalizedMessage( 502416 );
			}

			public override void OnCancel( Mobile from )
			{
				from.SendLocalizedMessage( 502415 );
				if ( !m_Book.Deleted && from.InRange( m_Book.GetWorldLocation(), 1 ) )
				{
					from.CloseGump( typeof( GMRunebookGump ) );
					from.SendGump( new GMRunebookGump( from, m_Book ) );
				}
			}
		}

		public override void OnResponse( NetState state, RelayInfo info )
		{
			Mobile from = state.Mobile;
			if ( m_Book.Deleted || !from.InRange( m_Book.GetWorldLocation(), 1 ) || !Multis.DesignContext.Check( from ) ) return;
			int buttonID = info.ButtonID;
			if ( buttonID == 1 )
			{
				if ( m_Book.CheckAccess( from ) )
				{
					from.SendLocalizedMessage( 502414 );
					from.Prompt = new InternalPrompt( m_Book );
				}
				else from.SendLocalizedMessage( 502413 );
			}
			else
			{
				buttonID -= 2;
				int index = buttonID / 6;
				int type = buttonID % 6;
				if ( index >= 0 && index < m_Book.Entries.Count )
				{
					GMRunebookEntry e = (GMRunebookEntry)m_Book.Entries[index];
					switch ( type )
					{
						case 0:
						{
							if ( m_Book.CurCharges <= 0 )
							{
								from.CloseGump( typeof( GMRunebookGump ) );
								from.SendGump( new GMRunebookGump( from, m_Book ) );
								from.SendLocalizedMessage( 502412 );
							}
							else
							{
								int xLong = 0, yLat = 0;
								int xMins = 0, yMins = 0;
								bool xEast = false, ySouth = false;
								if ( Sextant.Format( e.Location, e.Map, ref xLong, ref yLat, ref xMins, ref yMins, ref xEast, ref ySouth ) )
								{
									string location = String.Format( "{0}° {1}'{2}, {3}° {4}'{5}", yLat, yMins, ySouth ? "S" : "N", xLong, xMins, xEast ? "E" : "W" );
									from.SendMessage( location );
								}
								from.Location = e.Location;
								from.Map = e.Map;
							}
							break;
						}
						case 1:
						{
							if ( m_Book.CheckAccess( from ) )
							{
								m_Book.DropRune( from, e, index );
								from.CloseGump( typeof( GMRunebookGump ) );
								from.SendGump( new GMRunebookGump( from, m_Book ) );
							}
							else from.SendLocalizedMessage( 502413 );
							break;
						}
						case 2:
						{
							if ( m_Book.CheckAccess( from ) )
							{
								m_Book.Default = e;
								from.CloseGump( typeof( GMRunebookGump ) );
								from.SendGump( new GMRunebookGump( from, m_Book ) );
								from.SendLocalizedMessage( 502417 );
							}
							else from.SendLocalizedMessage( 502413 );
							break;
						}
						case 3:
						{
							from.Location = e.Location;
							from.Map = e.Map;
							break;
						}
						case 4:
						{
							from.SendLocalizedMessage( 501024 );
							Effects.PlaySound( from.Location, from.Map, 0x20E );
							InternalItem firstGate = new InternalItem( e.Location, e.Map );
							firstGate.MoveToWorld( from.Location, from.Map );
							Effects.PlaySound( from.Location, from.Map, 0x20E );
							InternalItem secondGate = new InternalItem( from.Location, from.Map );
							secondGate.MoveToWorld( e.Location, e.Map );
							break;
						}
					}
				}
			}
		}

		private class InternalItem : Moongate
		{
			public override bool ShowFeluccaWarning{ get{ return true; } }
			public InternalItem( Point3D target, Map map ) : base( target, map )
			{
				Map = map;
				if ( map == Map.Felucca || map == Map.Tokuno ) ItemID = 0xDDA;
				Dispellable = false;
				InternalTimer t = new InternalTimer( this );
				t.Start();
			}

			public InternalItem(Serial serial) : base(serial){}
			public override void Serialize( GenericWriter writer ) { base.Serialize( writer ); }
			public override void Deserialize( GenericReader reader ) { base.Deserialize( reader ); Delete(); }

			private class InternalTimer : Timer
			{
				private Item m_Item;

				public InternalTimer( Item item ) : base( TimeSpan.FromSeconds( 30.0 ) )
				{
					Priority = TimerPriority.OneSecond;
					m_Item = item;
				}

				protected override void OnTick()
				{
					m_Item.Delete();
				}
			}
		}
	}
}