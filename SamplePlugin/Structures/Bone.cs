using System;
using System.Numerics;
using System.Collections.Generic;
using FFXIVClientStructs.Havok.Common.Base.Math.QsTransform;
using FFXIVClientStructs.Havok.Common.Base.Math.Quaternion;
using FFXIVClientStructs.Havok.Animation.Rig;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using Dalamud.Hooking;

namespace FirstPersonTweaks.Structures
{
    public unsafe struct stCommonSkelBoneList
    {
        public short e_root = -1;
        public short e_abdomen = -1;
        public short e_waist = -1;

        public short e_spine_a = -1;
        public short e_spine_b = -1;
        public short e_spine_c = -1;
        public short e_neck = -1;
        public short e_head = -1;

        public short e_collarbone_l = -1;
        public short e_arm_l = -1;
        public short e_forearm_l = -1;
        public short e_elbow_l = -1;
        public short e_hand_l = -1;
        public short e_wrist_l = -1;

        public short e_thumb_a_l = -1;
        public short e_thumb_b_l = -1;
        public short e_finger_index_a_l = -1;
        public short e_finger_index_b_l = -1;
        public short e_finger_middle_a_l = -1;
        public short e_finger_middle_b_l = -1;
        public short e_finger_ring_a_l = -1;
        public short e_finger_ring_b_l = -1;
        public short e_finger_pinky_a_l = -1;
        public short e_finger_pinky_b_l = -1;

        public short e_scabbard_l = -1;
        public short e_sheathe_l = -1;
        public short e_weapon_l = -1;

        public short e_collarbone_r = -1;
        public short e_arm_r = -1;
        public short e_forearm_r = -1;
        public short e_elbow_r = -1;
        public short e_hand_r = -1;
        public short e_wrist_r = -1;

        public short e_scabbard_r = -1;
        public short e_sheathe_r = -1;
        public short e_weapon_r = -1;

        public short e_thumb_a_r = -1;
        public short e_thumb_b_r = -1;
        public short e_finger_index_a_r = -1;
        public short e_finger_index_b_r = -1;
        public short e_finger_middle_a_r = -1;
        public short e_finger_middle_b_r = -1;
        public short e_finger_ring_a_r = -1;
        public short e_finger_ring_b_r = -1;
        public short e_finger_pinky_a_r = -1;
        public short e_finger_pinky_b_r = -1;


        public float armLength = 1.0f;
        public Dictionary<short, KeyValuePair<short, HashSet<short>>> layout = new Dictionary<short, KeyValuePair<short, HashSet<short>>>();

        public stCommonSkelBoneList(Skeleton* skeleton)
        {
            Dictionary<String, short> nameList = new Dictionary<String, short>();
            hkaSkeleton* hkaSkel = skeleton->SkeletonResourceHandles[0]->HavokSkeleton;
            //----
            // Gets all the bones relative to their parents/children
            //----
            for (short i = 0; i < hkaSkel->Bones.Length; i++)
            {
                short p = hkaSkel->ParentIndices[i];
                nameList[hkaSkel->Bones[i].Name!.String!] = i;

                if (!layout.ContainsKey(i))
                    layout.Add(i, new KeyValuePair<short, HashSet<short>>(p, new HashSet<short>()));
                if (p >= 0)
                    layout[p].Value.Add(i);
            }
            updateChildren(0);

            e_root = nameList["n_root"];
            e_abdomen = nameList["n_hara"];
            e_waist = nameList["j_kosi"];

            e_spine_a = nameList["j_sebo_a"];
            e_spine_b = nameList["j_sebo_b"];
            e_spine_c = nameList["j_sebo_c"];
            e_neck = nameList["j_kubi"];
            e_head = nameList["j_kao"];

            e_collarbone_l = nameList["j_sako_l"];
            e_arm_l = nameList["j_ude_a_l"];
            e_forearm_l = nameList["j_ude_b_l"];
            e_elbow_l = nameList["n_hhiji_l"];
            e_hand_l = nameList["j_te_l"];
            e_wrist_l = nameList["n_hte_l"];

            e_thumb_a_l = nameList["j_oya_a_l"];
            e_thumb_b_l = nameList["j_oya_b_l"];
            e_finger_index_a_l = nameList["j_hito_a_l"];
            e_finger_index_b_l = nameList["j_hito_b_l"];
            e_finger_middle_a_l = nameList["j_naka_a_l"];
            e_finger_middle_b_l = nameList["j_naka_b_l"];
            e_finger_ring_a_l = nameList["j_kusu_a_l"];
            e_finger_ring_b_l = nameList["j_kusu_b_l"];
            e_finger_pinky_a_l = nameList["j_ko_a_l"];
            e_finger_pinky_b_l = nameList["j_ko_b_l"];

            e_scabbard_l = nameList["j_buki_sebo_l"];
            e_sheathe_l = nameList["j_buki_kosi_l"];
            e_weapon_l = nameList["n_buki_l"];

            e_collarbone_r = nameList["j_sako_r"];
            e_arm_r = nameList["j_ude_a_r"];
            e_forearm_r = nameList["j_ude_b_r"];
            e_elbow_r = nameList["n_hhiji_r"];
            e_hand_r = nameList["j_te_r"];
            e_wrist_r= nameList["n_hte_r"];

            e_thumb_a_r = nameList["j_oya_a_r"];
            e_thumb_b_r = nameList["j_oya_b_r"];
            e_finger_index_a_r = nameList["j_hito_a_r"];
            e_finger_index_b_r = nameList["j_hito_b_r"];
            e_finger_middle_a_r = nameList["j_naka_a_r"];
            e_finger_middle_b_r = nameList["j_naka_b_r"];
            e_finger_ring_a_r = nameList["j_kusu_a_r"];
            e_finger_ring_b_r = nameList["j_kusu_b_r"];
            e_finger_pinky_a_r = nameList["j_ko_a_r"];
            e_finger_pinky_b_r = nameList["j_ko_b_r"];

            e_scabbard_r = nameList["j_buki_sebo_r"];
            e_sheathe_r = nameList["j_buki_kosi_r"];
            e_weapon_r = nameList["n_buki_r"];

            if (e_arm_l >= 0 && e_forearm_l >= 0)
                armLength = hkaSkel->ReferencePose[e_arm_l].Translation.Convert().Length() + hkaSkel->ReferencePose[e_forearm_l].Translation.Convert().Length();

            /*
            foreach (KeyValuePair<short, KeyValuePair<short, HashSet<short>>> item in layout)
            {
                Log!.Info($"parent {item.Value.Key} item {item.Key} {hkaSkel->Bones[item.Key].Name.String} children {item.Value.Value.Count}");
                string numbers = "";
                foreach (short innerItem in item.Value.Value)
                {
                    numbers += ", " + hkaSkel->Bones[innerItem].Name.String;
                }
                Log!.Info(numbers);
            }
            */
        }

        private HashSet<short> updateChildren(short index, short indent = 0)
        {
            HashSet<short> allChildren = new HashSet<short>();
            foreach (short item in layout[index].Value)
            {
                allChildren.Add(item);
                HashSet<short> retChildren = updateChildren(item, (short)(indent + 1));
                foreach (short retItem in retChildren)
                    allChildren.Add(retItem);
            }
            layout[index] = new KeyValuePair<short, HashSet<short>>(layout[index].Key, allChildren);
            return layout[index].Value;
        }
    }


    public unsafe class Bone
    {
        private float Deg2Rad = MathF.PI / 180.0f;
        private float Rad2Deg = 180.0f / MathF.PI;
        public BoneList boneKey = BoneList._root_;
        public short id = -1;
        public short parentId = -1;
        public hkQsTransformf transform = new hkQsTransformf();
        public hkQsTransformf reference = new hkQsTransformf();
        public hkQsTransformf worldBase = new hkQsTransformf();
        public Bone? parent = null;
        public Dictionary<int, Bone> children = new Dictionary<int, Bone>();
        public Matrix4x4 boneMatrix = Matrix4x4.Identity;
        public Matrix4x4 boneMatrixI = Matrix4x4.Identity;
        public Matrix4x4 localMatrix = Matrix4x4.Identity;
        public Matrix4x4 localMatrixI = Matrix4x4.Identity;
        public Vector3 boneStart = new Vector3();
        public Vector3 boneFinish = new Vector3();
        public bool useReference = false;
        public bool updatePosition = false;
        public bool updateRotation = false;
        public bool updateScale = false;
        public bool disableParent = false;
        public bool isSet = false;

        public Bone() { }

        public Bone(BoneList bKey, short kId, short pId, Bone? pBone, hkQsTransformf hkqTransform, hkQsTransformf hkqReference)
        {
            boneKey = bKey;
            id = kId;
            parentId = pId;
            transform = hkqTransform;
            reference = hkqReference;
            isSet = true;
            parent = pBone;
            if (parent != null)
                parent.children.Add(kId, this);
            CalculateMatrix();
        }

        public void setUpdates(bool position, bool rotation, bool scale, bool runChild = false)
        {
            updatePosition = position;
            updateRotation = rotation;
            updateScale = scale;

            if (runChild == true)
                foreach (KeyValuePair<int, Bone> child in children)
                    child.Value.setUpdates(position, rotation, scale, runChild);
        }

        public void CalculateMatrix(bool runChild = false)
        {
            setLocalMatrix();
            setWorld();

            if (runChild == true)
                foreach (KeyValuePair<int, Bone> child in children)
                    child.Value.CalculateMatrix(runChild);
        }

        public void setWorld(bool setMatrix = true, bool runChild = false, bool skipParent = false)
        {
            if (parent != null && !skipParent)
            {
                worldBase.Rotation = (parent.worldBase.Rotation.Convert() * transform.Rotation.Convert()).Convert();
                worldBase.Translation = (parent.worldBase.Translation.Convert() + Vector3.Transform(transform.Translation.Convert(), parent.worldBase.Rotation.Convert())).Convert();
            }
            else
            {
                worldBase.Rotation = transform.Rotation;
                worldBase.Translation = transform.Translation;
            }
            worldBase.Scale = transform.Scale;
            if(setMatrix)
                setWorldMatrix();

            if (runChild == true)
                foreach (KeyValuePair<int, Bone> child in children)
                    child.Value.setWorld(setMatrix, runChild, skipParent);
        }

        public void setWorldMatrix(bool runChild = false)
        {
            boneMatrix = Matrix4x4.CreateFromQuaternion(worldBase.Rotation.Convert());
            boneMatrix.Translation = worldBase.Translation.Convert();
            boneMatrix.SetScale(worldBase.Scale);
            Matrix4x4.Invert(boneMatrix, out boneMatrixI);
            
            boneStart = (parent != null) ? parent.boneFinish : Vector3.Zero;
            boneFinish = boneMatrix.Translation;

            if (runChild == true)
                foreach (KeyValuePair<int, Bone> child in children)
                    child.Value.setWorldMatrix(runChild);
        }

        public void setLocal(bool setMatrix = true, bool runChild = false, bool skipParent = false)
        {
            if (parent != null && !skipParent)
            {
                transform.Rotation = (Quaternion.Inverse(parent.worldBase.Rotation.Convert()) * worldBase.Rotation.Convert()).Convert();
                transform.Translation = (Vector3.Transform(worldBase.Translation.Convert() - parent.worldBase.Translation.Convert(), Quaternion.Inverse(parent.worldBase.Rotation.Convert()))).Convert();
            }
            else
            {
                transform.Rotation = worldBase.Rotation;
                transform.Translation = worldBase.Translation;
            }
            transform.Scale = worldBase.Scale;
            if (setMatrix)
                setLocalMatrix();

            if (runChild == true)
                foreach (KeyValuePair<int, Bone> child in children)
                    child.Value.setLocal(setMatrix, runChild, skipParent);
        }

        public void setLocalMatrix(bool runChild = false)
        {
            localMatrix = Matrix4x4.CreateFromQuaternion(transform.Rotation.Convert());
            localMatrix.Translation = transform.Translation.Convert();
            localMatrix.SetScale(transform.Scale);
            Matrix4x4.Invert(localMatrix, out localMatrixI);

            if (runChild == true)
                foreach (KeyValuePair<int, Bone> child in children)
                    child.Value.setLocalMatrix(runChild);
        }

        public void SetWorldFromBoneMatrix(bool runChild = false) 
        {
            worldBase.Translation = boneMatrix.Translation.Convert();
            worldBase.Rotation = Quaternion.CreateFromRotationMatrix(boneMatrix).Convert();
            worldBase.Scale = boneMatrix.GetScale().Convert();

            boneStart = (parent != null) ? parent.boneFinish : Vector3.Zero;
            boneFinish = boneMatrix.Translation;

            //setLocal(true);
            //setWorldMatrix();

            if (runChild == true)
                foreach (KeyValuePair<int, Bone> child in children)
                    child.Value.SetWorldFromBoneMatrix(runChild);
        }

        public void Inverse()
        {
            updatePosition = true;
            transform.Translation.X = transform.Translation.X * -1;
            transform.Translation.Y = transform.Translation.Y * -1;
            transform.Translation.Z = transform.Translation.Z * -1;
            transform.Translation.W = 0;

            Quaternion q = new Quaternion(transform.Rotation.X, transform.Rotation.W, transform.Rotation.Z, transform.Rotation.W);
            q = Quaternion.Inverse(q);
            updateRotation = true;
            //transform.Rotation.X = q.X;
            //transform.Rotation.Y = q.Y;
            //transform.Rotation.Z = q.Z;
            //transform.Rotation.W = q.W;
        }

        public void InverseChildren()
        {
            Inverse();
            foreach (KeyValuePair<int, Bone> child in children)
                child.Value.InverseChildren();
        }

        public void SetReference(bool calculateMatrix = true, bool runChild = false)
        {
            updatePosition = true;
            updateRotation = true;
            updateScale = true;

            transform.Translation = reference.Translation;
            transform.Rotation = reference.Rotation;
            transform.Scale = reference.Scale;

            if(calculateMatrix)
                CalculateMatrix();

            if (runChild == true)
                foreach (KeyValuePair<int, Bone> child in children)
                    child.Value.SetReference(calculateMatrix, runChild);
        }

        public void SetTransform(hkQsTransformf location, bool calculateMatrix = true, bool runChild = false)
        {
            updatePosition = true;
            updateRotation = true;
            updateScale = true;

            transform = location;

            if (calculateMatrix)
                CalculateMatrix();

            if (runChild == true)
                foreach (KeyValuePair<int, Bone> child in children)
                    child.Value.SetTransform(location, runChild);
        }

        public void SetTransform(Matrix4x4 location, bool calculateMatrix = true, bool runChild = false)
        {
            updatePosition = true;
            updateRotation = true;
            updateScale = true;

            transform.Translation = location.Translation.Convert();
            transform.Rotation = Quaternion.CreateFromRotationMatrix(location).Convert();
            transform.Scale = location.GetScale().Convert();

            if (calculateMatrix)
                CalculateMatrix();

            if (runChild == true)
                foreach (KeyValuePair<int, Bone> child in children)
                    child.Value.SetTransform(location, runChild);
        }

        public void SetScale(Vector3 scale, bool runChild = false)
        {
            updateScale = true;
            transform.Scale = scale.Convert();

            if (runChild == true)
                foreach (KeyValuePair<int, Bone> child in children)
                    child.Value.SetScale(scale, runChild);
        }

        public Matrix4x4 ToLocal(Matrix4x4 matrix)
        {
            if (parent != null)
                return parent.ToLocal(matrix * boneMatrixI);
            else
                return matrix * boneMatrixI;
        }

        public void Output(int indent = 0, bool runChild = false)
        {
            ToEulerAngles(transform.Rotation.Convert(), out float pitch, out float yaw, out float roll);

            Quaternion q = Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll);
            string spacer = new String(' ', indent * 2);
            Plugin.Log!.Info($"{spacer} {parentId} {id}| {(BoneListEn)boneKey} | {transform.Translation.X} {transform.Translation.Y} {transform.Translation.Z} | {pitch * Rad2Deg} {yaw * Rad2Deg} {roll * Rad2Deg} | {transform.Rotation.X} {transform.Rotation.Y} {transform.Rotation.Z} {transform.Rotation.W} | {q.X} {q.Y} {q.Z} {q.W}");
            if (runChild == true)
                foreach (KeyValuePair<int, Bone> child in children)
                    child.Value.Output(indent + 1, runChild);
        }      



        public Vector3 ToEulerAngles(hkQuaternionf q)
        {
            ToEulerAngles(new Quaternion(q.X, q.Y, q.Z, q.W), out float pitch, out float yaw, out float roll);
            return new Vector3(pitch, yaw, roll);
        }

        public Vector3 ToEulerAngles(Quaternion q)
        {
            ToEulerAngles(q, out float pitch, out float yaw, out float roll);
            return new Vector3(pitch, yaw, roll);
        }

        public void ToEulerAngles(Quaternion q, out float pitch, out float yaw, out float roll)
        {
            float sqw = q.W * q.W;
            float sqx = q.X * q.X;
            float sqy = q.Y * q.Y;
            float sqz = q.Z * q.Z;
            float unit = sqx + sqy + sqz + sqw; // if normalised is one, otherwise is correction factor
            float test = q.X * q.W - q.Y * q.Z;

            if (test > 0.49975f * unit)
            {   // singularity at north pole
                yaw = 2f * MathF.Atan2(q.Y, q.X);
                pitch = MathF.PI / 2f;
                roll = 0;
                return;
            }
            if (test < -0.49975f * unit)
            {   // singularity at south pole
                yaw = -2f * MathF.Atan2(q.Y, q.X);
                pitch = -MathF.PI / 2f;
                roll = 0;
                return;
            }

            Quaternion q1 = new Quaternion(q.W, q.Z, q.X, q.Y);
            yaw = 1 * MathF.Atan2(2f * (q1.X * q1.W + q1.Y * q1.Z), 1f - 2f * (q1.Z * q1.Z + q1.W * q1.W));   // Yaw
            pitch = 1 * MathF.Asin(2f * (q1.X * q1.Z - q1.W * q1.Y));                                         // Pitch
            roll = 1 * MathF.Atan2(2f * (q1.X * q1.Y + q1.Z * q1.W), 1f - 2f * (q1.Y * q1.Y + q1.Z * q1.Z));  // Roll
        }

        Quaternion FromToRotation(Vector3 aFrom, Vector3 aTo)
        {
            Vector3 axis = Vector3.Cross(aFrom, aTo);
            float angle = Angle(aFrom, aTo);
            return AngleAxis(Vector3.Normalize(axis), angle);
        }

        float Angle(Vector3 from, Vector3 to)
        {
            float kEpsilonNormalSqrt = 1e-15F;
            // sqrt(a) * sqrt(b) = sqrt(a * b) -- valid for real numbers
            float denominator = (float)Math.Sqrt(from.LengthSquared() * to.LengthSquared());
            if (denominator < kEpsilonNormalSqrt)
                return 0F;

            float dot = Math.Clamp(Vector3.Dot(from, to) / denominator, -1f, 1f);
            return ((float)Math.Acos(dot)) * Rad2Deg;
        }

        Quaternion AngleAxis(Vector3 aAxis, float aAngle)
        {
            aAxis = Vector3.Normalize(aAxis);
            float rad = aAngle * Deg2Rad * 0.5f;
            aAxis *= MathF.Sin(rad);
            return new Quaternion(aAxis.X, aAxis.Y, aAxis.Z, MathF.Cos(rad));
        }
    }
          
}
