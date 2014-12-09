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
    /// Handle all Game World Environment
    /// </summary>
    public static class Game
    {
        private static int PlayerAutoID = 0x1000000;
        private static int NPCAutoID = 0x2000000;
        private static int MonsterAutoID = 0x40000000;

        #region Properties

        public static Map Map = new Map();
        public static List<Player> Players = new List<Player>();
        public static List<Creature> Creatures = new List<Creature>();

        #endregion

        #region Get

        /// <summary>
        /// Get a player with the given name.
        /// </summary>
        /// <param name="Name"></param>
        /// <returns>If no character is found returns NULL</returns>
        public static Player getPlayer(string Name)
        {
            // TODO: Use IEnumerable Query Instead
            foreach (Player p in Players)
            {
                if (p.Name == Name)
                {
                    return p;
                }
            }

            return null;
        }

        /// <summary>
        /// Get a creature with the given id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>If no creature is found returns NULL</returns>
        public static Creature getCreature(int id)
        {
            // TODO: Use IEnumerable Query Instead
            foreach (Creature p in Creatures)
            {
                if (p.Id == id)
                {
                    return p;
                }
            }

            return null;
        }

        /// <summary>
        /// Get current world ambiente
        /// </summary>
        /// <returns></returns>
        public static Light getWorldAmbiente()
        {
            Light ambiente = new Light();
            ambiente.Radius = 255;
            ambiente.Color = 215;
            return ambiente;
        }

        /// <summary>
        /// Get players in a certain area with the given radius
        /// </summary>
        /// <param name="centerPos"></param>
        /// <param name="MultiFloor"></param>
        /// <param name="MinRangeX"></param>
        /// <param name="MaxRangeX"></param>
        /// <param name="MinRangeY"></param>
        /// <param name="MaxRangeY"></param>
        /// <returns></returns>
        public static void GetSpectators(HashSet<Player> Spectators, Position centerPos, bool MultiFloor = false, int MinRangeX = 11, int MaxRangeX = 11, int MinRangeY = 11, int MaxRangeY = 11)
        {
            foreach (Player p in Players)
            {
                if (p.Connection == null || !p.Connection.LoggedIn)
                {
                    continue;
                }

                int MinRangeZ = centerPos.Z;
                int MaxRangeZ = centerPos.Z;
                if (MultiFloor)
                {
                    if (centerPos.Z > 7)
                    {
                        MinRangeZ = Math.Max(centerPos.Z - 2, 0);
                        MaxRangeZ = Math.Min(centerPos.Z + 2, 15);
                    }
                    else if (centerPos.Z == 6)
                    {
                        MinRangeZ = 0;
                        MaxRangeZ = 8;
                    }
                    else if (centerPos.Z == 7)
                    {
                        MinRangeZ = 0;
                        MaxRangeZ = 9;
                    }
                    else
                    {
                        MinRangeZ = 0;
                        MaxRangeZ = 7;
                    }
                }

                if (p.Position.X - centerPos.X > -MinRangeX && p.Position.X - centerPos.X < MaxRangeX && p.Position.Y - centerPos.Y > -MinRangeY && p.Position.Y - centerPos.Y < MaxRangeY && (p.Position.Z - centerPos.Z > -MinRangeZ || p.Position.Z - centerPos.Z < MaxRangeZ))
                {
                    Spectators.Add(p);
                }
            }
        }

        #endregion

        #region Functioning

        /// <summary>
        /// Check if a player can spawn at a certain position
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public static bool canPlayerSpawnAt(Player player, Position pos)
        {
            Tile tile = Map.getTile(pos);
            if (tile == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Set a player on the map
        /// </summary>
        /// <param name="player"></param>
        /// <param name="pos"></param>
        public static void setPlayerOnMap(Player player, Position pos)
        {
            Player p = getPlayer(player.Name);
            if (p != null)
            {
                p.Connection.Disconnect();
                p.Connection = player.Connection;
                player = p;
            }
            else
            {
                // Setting Player ID
                player.Id = PlayerAutoID++;
                // Add Player to players list
                Players.Add(player);
                Tile tile = Map.getTile(pos); 
                // No checking here if tile is null as this method is called after we've already checked.
                player.StandingTile = tile;
                player.Position = tile.Position;
                tile.AddCreature(player);
            }
        }

        /// <summary>
        /// Remove a creature from the map
        /// </summary>
        /// <param name="player"></param>
        public static void RemoveCreature(Creature creature)
        {
            Creatures.Remove(creature);
            creature.StandingTile.RemoveCreature(creature);
        }

        #endregion

        #region Creature Events

        /// <summary>
        /// Moves a creature from the map with the given direction to another tile
        /// </summary>
        /// <param name="creature"></param>
        /// <param name="direction"></param>
        public static void MoveCreature(Creature creature, Direction direction)
        {
            Tile toTile = Map.getTile(creature.Position.getAdjacentPosition(direction));
            if (toTile == null)
            {
                return;
            }

            creature.StandingTile.MoveCreature(creature, toTile);
        }

        #endregion
    }
}
