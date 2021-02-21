using System;
using System.Collections;
using System.Collections.Generic;
using Server;
using Server.Gumps;
using Server.Network;
using Server.Multis;
using Server.Engines.Craft;
using Server.ContextMenus;
using Server.Regions;

namespace Server.Items
{
	public class GMRunebook : Item, ISecurable
	{
		public static readonly TimeSpan UseDelay = TimeSpan.FromSeconds( 1.0 );

		private ArrayList m_Entries;
		private string m_Description;
		private int m_CurCharges, m_MaxCharges;
		private int m_DefaultIndex;
		private SecureLevel m_Level;
		private Mobile m_Crafter;

		private DateTime m_NextUse;

		[CommandProperty( AccessLevel.GameMaster )]
		public DateTime NextUse
		{
			get{ return m_NextUse; }
			set{ m_NextUse = value; }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public Mobile Crafter
		{
			get{ return m_Crafter; }
			set{ m_Crafter = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public SecureLevel Level
		{
			get{ return m_Level; }
			set{ m_Level = value; }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public string Description
		{
			get { return m_Description; }
			set { m_Description = value; InvalidateProperties(); }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int CurCharges
		{
			get { return m_CurCharges; }
			set { m_CurCharges = value; }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int MaxCharges
		{
			get { return m_MaxCharges; }
			set { m_MaxCharges = value; }
		}

		public override int LabelNumber{ get{ return 1041267; } }

		[Constructable]
		public GMRunebook( int maxCharges ) : base( 0x22C5 )
		{
			Name = "GM Runebook";
			Weight = 1.0;
			LootType = LootType.Blessed;
			Hue = 0x461;
			Layer = Layer.Invalid ;
			m_Entries = new ArrayList();
			m_MaxCharges = maxCharges;
			m_CurCharges = maxCharges;
			m_DefaultIndex = -1;
			m_Level = SecureLevel.CoOwners;
		}

		[Constructable]
		public GMRunebook() : this( 60000 ){}

		public ArrayList Entries { get { return m_Entries; } }

		public GMRunebookEntry Default
		{
			get
			{
				if ( m_DefaultIndex >= 0 && m_DefaultIndex < m_Entries.Count ) return (GMRunebookEntry)m_Entries[m_DefaultIndex];
				return null;
			}
			set
			{
				if ( value == null ) m_DefaultIndex = -1;
				else m_DefaultIndex = m_Entries.IndexOf( value );
			}
		}

		public override void GetContextMenuEntries( Mobile from, List<ContextMenuEntry> list )
		{
			base.GetContextMenuEntries( from, list );
			SetSecureLevelEntry.AddTo( from, this, list );
		}

		public GMRunebook(Serial serial) : base(serial){}
		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 );
			writer.Write( m_Crafter );
			writer.Write( (int) m_Level );
			writer.Write( m_Entries.Count );
			for ( int i = 0; i < m_Entries.Count; ++i )
				((GMRunebookEntry)m_Entries[i]).Serialize( writer );
			writer.Write( m_Description );
			writer.Write( m_CurCharges );
			writer.Write( m_MaxCharges );
			writer.Write( m_DefaultIndex );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
			m_Crafter = reader.ReadMobile();
			m_Level = (SecureLevel)reader.ReadInt();
			int count = reader.ReadInt();
			m_Entries = new ArrayList( count );
			for ( int i = 0; i < count; ++i )
			{
				m_Entries.Add( new GMRunebookEntry( reader ) );
			}
			m_Description = reader.ReadString();
			m_CurCharges = reader.ReadInt();
			m_MaxCharges = reader.ReadInt();
			m_DefaultIndex = reader.ReadInt();
		}

		public void DropRune( Mobile from, GMRunebookEntry e, int index )
		{
			if ( m_DefaultIndex == index ) m_DefaultIndex = -1;
			m_Entries.RemoveAt( index );
			GMRecallRune rune = new GMRecallRune();
			rune.Target = e.Location;
			rune.TargetMap = e.Map;
			rune.Description = e.Description;
			rune.Marked = true;
			from.AddToBackpack( rune );
			from.SendLocalizedMessage( 502421 );
		}

		public bool IsOpen( Mobile toCheck )
		{
			NetState ns = toCheck.NetState;
			if ( ns != null )
			{
				foreach ( Gump gump in ns.Gumps )
				{
					GMRunebookGump bookGump = gump as GMRunebookGump;
					if ( bookGump != null && bookGump.Book == this ) return true;
				}
			}
			return false;
		}

		public override bool DisplayLootType{ get{ return true; } }

		public override void GetProperties( ObjectPropertyList list )
		{
			base.GetProperties( list );
			if ( m_Crafter != null ) list.Add( 1050043, m_Crafter.Name );
			if ( m_Description != null && m_Description.Length > 0 ) list.Add( m_Description );
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( !(from.AccessLevel >= AccessLevel.GameMaster) ) return;
			if ( from.InRange( GetWorldLocation(), 1 ) )
			{
				if ( DateTime.Now < NextUse )
				{
					from.SendLocalizedMessage( 502406 );
					return;
				}

				from.CloseGump( typeof( GMRunebookGump ) );
				from.SendGump( new GMRunebookGump( from, this ) );
			}
		}

		public virtual void OnTravel()
		{
			NextUse = DateTime.Now + UseDelay;
		}

		public override void OnAfterDuped( Item newItem )
		{
			GMRunebook book = newItem as GMRunebook;
			if ( book == null ) return;
			book.m_Entries = new ArrayList();
			for ( int i = 0; i < m_Entries.Count; i++ )
			{
				GMRunebookEntry entry = m_Entries[i] as GMRunebookEntry;
				book.m_Entries.Add( new GMRunebookEntry( entry.Location, entry.Map, entry.Description ) );
			}
		}

		public bool CheckAccess( Mobile m )
		{
			if ( !IsLockedDown || m.AccessLevel >= AccessLevel.GameMaster ) return true;
			return false;
		}

		public override bool OnDragDrop( Mobile from, Item dropped )
		{
			if ( dropped is GMRecallRune)
			{
				if ( !CheckAccess( from ) ) from.SendLocalizedMessage( 502413 );
				else if ( IsOpen( from ) ) from.SendLocalizedMessage( 1005571 );
				else if ( m_Entries.Count < 16 )
				{
					GMRecallRune rune = (GMRecallRune)dropped;
					if ( rune.Marked && rune.TargetMap != null )
					{
						m_Entries.Add( new GMRunebookEntry( rune.Target, rune.TargetMap, rune.Description ) );
						dropped.Delete();
						from.Send( new PlaySound( 0x42, GetWorldLocation() ) );
						string desc = rune.Description;
						if ( desc == null || (desc = desc.Trim()).Length == 0 ) desc = "(indescript)";
						from.SendMessage( desc );
						return true;
					}
					else from.SendLocalizedMessage( 502409 );
				}
				else from.SendLocalizedMessage( 502401 );
			}
			else if ( dropped is RecallScroll )
			{
				if ( m_CurCharges < m_MaxCharges )
				{
					from.Send( new PlaySound( 0x249, GetWorldLocation() ) );
					int amount = dropped.Amount;
					if ( amount > (m_MaxCharges - m_CurCharges) )
					{
						dropped.Consume( m_MaxCharges - m_CurCharges );
						m_CurCharges = m_MaxCharges;
					}
					else
					{
						m_CurCharges += amount;
						dropped.Delete();
						return true;
					}
				}
				else from.SendLocalizedMessage( 502410 );
			}
			return false;
		}
		#region ICraftable Members

		public int OnCraft( int quality, bool makersMark, Mobile from, CraftSystem craftSystem, Type typeRes, BaseTool tool, CraftItem craftItem, int resHue )
		{
			int charges = 5 + quality + (int)(from.Skills[SkillName.Inscribe].Value / 30);
			MaxCharges = charges * 2;
			if ( makersMark ) Crafter = from;
			return quality;
		}
		#endregion
	}

	public class GMRunebookEntry
	{
		public Point3D m_RuneLocation;
		public Map m_RuneMap;
		public string m_RuneDescription;

		public Point3D Location { get{ return m_RuneLocation; } set{ m_RuneLocation = value; } }

		public Map Map { get{ return m_RuneMap; } set{ m_RuneMap = value; } }

		public string Description { get{ return m_RuneDescription; } set{ m_RuneDescription = value; } }

		public GMRunebookEntry( Point3D loc, Map map, string desc )
		{
			m_RuneLocation = loc;
			m_RuneMap = map;
			m_RuneDescription = desc;
		}

		public GMRunebookEntry( GenericReader reader )
		{
			int version = reader.ReadByte();
			m_RuneLocation = reader.ReadPoint3D();
			m_RuneMap = reader.ReadMap();
			m_RuneDescription = reader.ReadString();
		}

		public void Serialize( GenericWriter writer )
		{
			writer.Write( (byte) 0 );
			writer.Write( m_RuneLocation );
			writer.Write( m_RuneMap );
			writer.Write( m_RuneDescription );
		}
	}
}