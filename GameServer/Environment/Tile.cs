﻿/**
 * OTSharp 7.6 - a free and open-source MMORPG server emulator
 * Copyright (C) 2014  Daniel Alejandro <alejandrodemujica@gmail.com>
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along
 * with this program; if not, write to the Free Software Foundation, Inc.,
 * 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameServer.Utils;

namespace GameServer.Environment
{
    /// <summary>
    /// Tile in the Game World Map
    /// </summary>
    public class Tile
    {
        #region Properties

        /// <summary>
        /// Ground Tile Item, like grass, water, lava, etc...
        /// A tile can only contain one ground item
        /// </summary>
        public Item GroundItem { get; set; }
        /// <summary>
        /// This tile position on the game map
        /// </summary>
        public Position Position { get; set; }
        /// <summary>
        /// List of existing creatures on this tile
        /// </summary>
        public List<Creature> Creatures = new List<Creature>();

        #endregion

        #region Booleans

        /// <summary>
        /// Check if there are creatures on this tile
        /// </summary>
        /// <returns></returns>
        public bool hasNoCreatures()
        {
            return Creatures.Count == 0;
        }

        #endregion

        #region Functioning

        /// <summary>
        /// Add a creature to this tile
        /// </summary>
        /// <param name="creature"></param>
        /// <param name="runEvent">Whether or not should run the onCreatureAdd event which announces to spectators</param>
        public void AddCreature(Creature creature, bool runEvent = true)
        {
            Creatures.Add(creature);
            creature.StandingTile = this;
            if (runEvent)
            {
                onCreatureAdd(creature);
            }
        }

        /// <summary>
        /// Remove a creature from this tile
        /// </summary>
        /// <param name="creature"></param>
        /// <param name="runEvent">Whether or not should run the onCreatureRemove event which announces to spectators</param>
        public void RemoveCreature(Creature creature, bool runEvent = true)
        {
            Creatures.Remove(creature);
            if (runEvent)
            {
                onCreatureRemove(creature);
            }
        }

        /// <summary>
        /// Moves a creature from this tile to another
        /// </summary>
        /// <param name="creature"></param>
        /// <param name="toTile"></param>
        public void MoveCreature(Creature creature, Tile toTile)
        {
            toTile.AddCreature(creature, false);
            onCreatureMove(creature, toTile);
            RemoveCreature(creature, false);
        }

        #endregion

        #region Events & Announcing

        private void onCreatureAdd(Creature creature)
        {
            HashSet<Creature> players = new HashSet<Creature>();
            Game.GetCreatureSpectators(players, Position, true);
            foreach (Creature c in players)
            {
                if (c is Player)
                {
                    Player player = (Player)c;
                    player.Connection.SendAddCreature(creature, 1);
                    player.onCreatureAppear(creature);
                }

                c.onCreatureAppear(creature);
            }
        }

        private void onCreatureRemove(Creature creature)
        {
            HashSet<Creature> players = new HashSet<Creature>();
            Game.GetCreatureSpectators(players, Position, true);
            foreach (Creature c in players)
            {
                if (c is Player)
                {
                    Player player = (Player)c;
                    player.Connection.SendRemoveCreature(creature, 1);
                    player.onCreatureDisappear(creature);
                }

                c.onCreatureDisappear(creature);
            }
        }

        private void onCreatureMove(Creature creature, Tile toTile)
        {
            HashSet<Creature> players = new HashSet<Creature>();
            Game.GetCreatureSpectators(players, Position, true);
            Game.GetCreatureSpectators(players, toTile.Position, true);
            foreach (Creature c in players)
            {
                if (c is Player)
                {
                    Player player = (Player)c;
                    player.Connection.SendCreatureMove(creature, toTile, 1, this, 1);
                }

                c.onCreatureMove(creature, this, toTile);
            }
        }

        #endregion
    }
}
